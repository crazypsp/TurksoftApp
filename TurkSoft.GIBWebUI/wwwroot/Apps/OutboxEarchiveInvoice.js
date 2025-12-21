// wwwroot/apps/OutboxEarchiveInvoice.js
// Bu dosya server-side DataTable ajax/search kullanmaz.
// ERP: GibInvoice + (GibInvoiceOperationLog || GibGibInvoiceOperationLog)
// Log: OperationName=SendEArchiveJson, ErrorCode=200
// Portal: /api/TurkcellEFatura/earchive/status/{id}?userId=..  -> Durum (Gib/Cevap)
// Portal: /api/TurkcellEFatura/earchive/pdf/{id}?userId=..&standardXslt=true -> PDF indir

(function (global) {
    'use strict';

    const $ = global.jQuery;

    // ---------------------------
    // UI helpers
    // ---------------------------
    function err(m) { if (global.toastr?.error) global.toastr.error(m); else alert(m); }

    function num(v) {
        const n = Number(v);
        return Number.isFinite(n) ? n : 0;
    }
    function fmt(n) {
        return (Number(n) || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
    function pad2(x) { return String(x).padStart(2, '0'); }

    // dd.mm.yyyy (veya dd.mm.yy) -> yyyy-MM-dd
    function toIsoDateTR(str) {
        if (!str) return '';
        const p = String(str).trim().split('.');
        if (p.length !== 3) return '';
        const dd = pad2(p[0]);
        const mm = pad2(p[1]);
        let yy = String(p[2]).trim();
        if (yy.length === 2) yy = '20' + yy;
        if (yy.length !== 4) return '';
        return `${yy}-${mm}-${dd}`;
    }

    // ISO veya "yyyy-MM-dd HH:mm:ss.fffffff" gibi .NET formatlarını güvenli parse eder.
    function parseAnyDate(val) {
        if (!val) return null;
        const s = String(val).trim();

        let t = Date.parse(s);
        if (Number.isFinite(t)) return new Date(t);

        const mDotNet = s.match(/Date\((\d+)\)/i);
        if (mDotNet) {
            const ms = parseInt(mDotNet[1], 10);
            if (!isNaN(ms)) return new Date(ms);
        }

        const m = s.match(
            /^(\d{4})-(\d{2})-(\d{2})[ T](\d{2}):(\d{2})(?::(\d{2})(?:\.(\d+))?)?$/
        );
        if (m) {
            const Y = parseInt(m[1], 10);
            const M = parseInt(m[2], 10) - 1;
            const D = parseInt(m[3], 10);
            const h = parseInt(m[4], 10);
            const mi = parseInt(m[5], 10);
            const se = parseInt(m[6] || '0', 10);
            const frac = (m[7] || '');
            const ms = frac ? parseInt(frac.substring(0, 3).padEnd(3, '0'), 10) : 0;
            return new Date(Y, M, D, h, mi, se, ms);
        }

        return null;
    }

    function inRange(d, startIso, endIso) {
        if (!d) return false;
        if (!startIso && !endIso) return true;

        const s = startIso ? parseAnyDate(`${startIso} 00:00:00`) : null;
        const e = endIso ? parseAnyDate(`${endIso} 23:59:59.999`) : null;

        if (s && d < s) return false;
        if (e && d > e) return false;
        return true;
    }

    function safeJsonParse(x) {
        if (!x) return null;
        if (typeof x === 'object') return x;
        const s = String(x).trim();
        if (!s) return null;
        try { return JSON.parse(s); } catch { return null; }
    }

    function pick(o, keys, defVal = null) {
        if (!o) return defVal;
        for (const k of keys) {
            if (o[k] !== undefined && o[k] !== null) return o[k];
        }
        return defVal;
    }

    // ---------------------------
    // API helpers (ERP: window.__API_BASE)
    // ---------------------------
    function normalizeBaseUrl() {
        let base = String(global.__API_BASE || '').trim();
        base = base.replace(/\/+$/, '');
        return base;
    }

    function buildApiUrl(resource) {
        const base = normalizeBaseUrl();
        if (!base) return `/api/v1/${resource}`;
        if (/\/api\/v1$/i.test(base)) return `${base}/${resource}`;
        return `${base}/api/v1/${resource}`;
    }

    async function fetchJson(url, opts) {
        const o = opts || {};
        const res = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            credentials: o.credentials || 'include',
            mode: o.mode || 'cors'
        });

        if (!res.ok) throw new Error(`HTTP ${res.status} (${url})`);

        const text = await res.text();
        if (!text) return null;

        try { return JSON.parse(text); } catch { return text; }
    }

    // ---------------------------
    // Gib Portal helpers (window.gibPortalApiBaseUrl)
    // ---------------------------
    function normalizeGibPortalBaseUrl() {
        // Beklenen: https://localhost:7151/api  veya prod'da .../api
        let base = String(global.gibPortalApiBaseUrl || '').trim();
        if (!base) base = `${global.location.origin}/api`;

        base = base.replace(/\/+$/, '');

        // base zaten /api ile bitiyorsa dokunma; değilse /api ekle
        if (!/\/api$/i.test(base)) base = `${base}/api`;

        return base;
    }

    function buildGibPortalUrl(pathAfterApi) {
        const base = normalizeGibPortalBaseUrl();
        let p = String(pathAfterApi || '').trim();
        p = p.replace(/^\/+/, '');
        p = p.replace(/^api\//i, '');
        return `${base}/${p}`;
    }

    async function fetchJsonPortal(url) {
        try {
            // Cross-origin CORS sorunlarında include patlatır; omit daha güvenli
            return await fetchJson(url, { credentials: 'omit', mode: 'cors' });
        } catch (e) {
            if (e && String(e).includes('TypeError')) {
                throw new Error(`İstek atılamadı (Failed to fetch). URL: ${url}. Muhtemel sebep: CORS/SSL/Network.`);
            }
            throw e;
        }
    }

    // ---------------------------
    // Status mappings (e-Arşiv)
    // ---------------------------
    const EArchiveStatusText = {
        0: 'Taslak',
        20: 'Kuyruk',
        30: "Gib'e Gönderiliyor",
        40: 'Hata',
        50: "Gib'e İletildi",
        60: 'Onaylandı',
        100: 'e-Arşiv İptal'
    };

    function formatStatusResponse(r) {
        if (!r || typeof r !== 'object') return '';

        const st = (r.status !== undefined && r.status !== null) ? num(r.status) : null;

        // İstek: status=0 -> sadece Taslak
        if (st === 0) return 'Taslak';

        const stTxt = (st !== null) ? (EArchiveStatusText[st] || 'Bilinmeyen') : '—';
        const m = String(r.message || '').trim();

        const left = `${st !== null ? st : '—'}(${stTxt})`;
        return m ? `${left} - ${m}` : left;
    }

    // ---------------------------
    // Current user
    // ---------------------------
    function getCurrentUserId() {
        let userId = 0;

        const hdn = document.getElementById('hdnUserId');
        if (hdn?.value) {
            const p = parseInt(hdn.value, 10);
            if (!isNaN(p) && p > 0) userId = p;
        }

        if (!userId && typeof global.currentUserId === 'number' && global.currentUserId > 0) {
            userId = global.currentUserId;
        }

        if (!userId) {
            try {
                const stored =
                    sessionStorage.getItem('CurrentUserId') ||
                    sessionStorage.getItem('currentUserId') ||
                    sessionStorage.getItem('UserId');
                if (stored) {
                    const p = parseInt(stored, 10);
                    if (!isNaN(p) && p > 0) userId = p;
                }
            } catch { }
        }

        if (!userId) throw new Error('Kullanıcı Id (userId) bulunamadı.');
        return userId;
    }

    // ---------------------------
    // State
    // ---------------------------
    const State = {
        userId: 0,
        items: [],
        dt: null,
        rowByLogId: {}
    };

    // ---------------------------
    // Mapping
    // ---------------------------
    function mapInvoice(invRaw) {
        const inv = invRaw || {};

        const id = num(pick(inv, ['Id', 'id']));
        const userId = num(pick(inv, ['UserId', 'userId']));

        const invoiceNo = String(pick(inv, ['InvoiceNo', 'invoiceNo'], '') || '');
        const invoiceDate = pick(inv, ['InvoiceDate', 'invoiceDate'], '');

        const total = num(pick(inv, ['Total', 'total'], 0));

        const type = pick(inv, ['Type', 'type'], '');
        const typeText = (type === 0 || type === '0') ? 'SATIŞ' : String(type ?? '');

        const c = inv.customer || inv.Customer || null;
        const customerName = c ? `${c.name || ''} ${c.surname || ''}`.trim() : '';
        const taxNo = c ? (c.taxNo || c.taxno || '') : '';

        let kdv = 0;
        const taxes = inv.invoicesTaxes || inv.InvoicesTaxes || [];
        if (Array.isArray(taxes)) {
            for (const tx of taxes) kdv += num(tx.amount ?? tx.Amount ?? 0);
        }

        // E-Posta Durumu (varsa)
        const hasEmail = pick(inv, ['HasEMail', 'hasEMail', 'HasEmail', 'hasEmail'], null);
        let emailStatus = '-';
        if (hasEmail === true || String(hasEmail).toUpperCase() === 'YES') emailStatus = 'Mail Gönderildi';
        else if (hasEmail === false || String(hasEmail).toUpperCase() === 'NO') emailStatus = 'Mail Gönderilmedi';

        // Muhasebeleştirme (varsa)
        const isAcc = pick(inv, ['IsAccount', 'isAccount'], null);
        let isAccountText = '-';
        if (isAcc === true || String(isAcc).toUpperCase() === 'YES') isAccountText = 'Evet';
        else if (isAcc === false || String(isAcc).toUpperCase() === 'NO') isAccountText = 'Hayır';

        return {
            id,
            userId,
            invoiceNo,
            invoiceDate,
            total,
            typeText,
            customerName,
            taxNo,
            kdv,
            emailStatus,
            isAccountText,
            raw: inv
        };
    }

    function mapLog(logRaw) {
        const lg = logRaw || {};
        return {
            id: num(pick(lg, ['Id', 'id'])),
            invoiceId: num(pick(lg, ['InvoiceId', 'invoiceId'])),
            operationName: String(pick(lg, ['OperationName', 'operationName'], '') || ''),
            errorCode: pick(lg, ['ErrorCode', 'errorCode'], ''),
            rawResponseJson: pick(lg, ['RawResponseJson', 'rawResponseJson'], ''),
            createdAt: pick(lg, ['CreatedAt', 'createdAt'], ''),
            raw: lg
        };
    }

    // ---------------------------
    // UI params + default date range
    // ---------------------------
    function setDefaultMonthDatesIfEmpty() {
        if (!$) return;

        const s = ($('#IssueDateStart').val() || '').trim();
        const e = ($('#IssueDateStartEnd').val() || '').trim();
        if (s || e) return;

        const now = new Date();
        const start = new Date(now.getFullYear(), now.getMonth(), 1);
        const endDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);

        $('#IssueDateStart').val(`${pad2(start.getDate())}.${pad2(start.getMonth() + 1)}.${start.getFullYear()}`);
        $('#IssueDateStartEnd').val(`${pad2(endDay.getDate())}.${pad2(endDay.getMonth() + 1)}.${endDay.getFullYear()}`);
    }

    function splitMultiId(str) {
        const s = String(str || '').trim();
        if (!s) return [];
        return s.split(',').map(x => x.trim()).filter(Boolean);
    }

    function buildParamsFromUI() {
        setDefaultMonthDatesIfEmpty();

        return {
            userId: State.userId,
            invoiceDateStartIso: toIsoDateTR($('#IssueDateStart').val()),
            invoiceDateEndIso: toIsoDateTR($('#IssueDateStartEnd').val()),

            documentId: String($('#DocumentId').val() || '').trim(),
            uuid: String($('#UUID').val() || '').trim(),

            targetIdentifier: String($('#TargetIdentifier').val() || '').trim(),
            targetIdentifierMulti: !!$('#TargetIdentifierChk').is(':checked'),
            targetTitle: String($('#TargetTitle').val() || '').trim(),

            // “Hata / Hatasız” filtre: status hydrate edildikten sonra uygulanır
            statusFilter: String($('#Status').val() || '').trim()
        };
    }

    // ---------------------------
    // Load + join (ERP)
    // ---------------------------
    async function loadDataAndJoin(ui) {
        const invoiceUrl = buildApiUrl('GibInvoice');

        async function fetchLogsWithFallback() {
            try {
                return await fetchJson(buildApiUrl('GibInvoiceOperationLog'), { credentials: 'include' });
            } catch {
                return await fetchJson(buildApiUrl('GibGibInvoiceOperationLog'), { credentials: 'include' });
            }
        }

        const [invRes, logRes] = await Promise.all([
            fetchJson(invoiceUrl, { credentials: 'include' }),
            fetchLogsWithFallback()
        ]);

        const invAll = Array.isArray(invRes) ? invRes : (invRes?.items || []);
        const logAll = Array.isArray(logRes) ? logRes : (logRes?.items || []);

        const invoicesAll = (invAll || []).map(mapInvoice);

        // ✅ Invoice filtre: UserId + date + faturaNo + firma/vkn
        const invoices = invoicesAll.filter(inv => {
            if (num(inv.userId) !== num(ui.userId)) return false;

            const d = parseAnyDate(inv.invoiceDate);
            if (!inRange(d, ui.invoiceDateStartIso, ui.invoiceDateEndIso)) return false;

            if (ui.documentId) {
                if (!String(inv.invoiceNo || '').toLowerCase().includes(ui.documentId.toLowerCase())) return false;
            }

            if (ui.targetTitle) {
                if (!String(inv.customerName || '').toLowerCase().includes(ui.targetTitle.toLowerCase())) return false;
            }

            if (ui.targetIdentifier) {
                const tax = String(inv.taxNo || '').trim();
                if (!tax) return false;

                if (ui.targetIdentifierMulti) {
                    const list = splitMultiId(ui.targetIdentifier);
                    if (!list.length) return false;
                    if (!list.includes(tax)) return false;
                } else {
                    if (tax !== String(ui.targetIdentifier).trim()) return false;
                }
            }

            return true;
        });

        const invById = new Map(invoices.map(x => [x.id, x]));

        // ✅ Log: SendEArchiveJson + ErrorCode=200 + en güncel
        const bestLogByInvoiceId = new Map();

        for (const r of (logAll || [])) {
            const lg = mapLog(r);
            if (!lg.invoiceId) continue;

            if (lg.operationName !== 'SendEArchiveJson') continue;
            if (String(lg.errorCode ?? '').trim() !== '200') continue;
            if (!invById.has(lg.invoiceId)) continue;

            const prev = bestLogByInvoiceId.get(lg.invoiceId);
            if (!prev) {
                bestLogByInvoiceId.set(lg.invoiceId, lg);
                continue;
            }

            const pd = parseAnyDate(prev.createdAt);
            const nd = parseAnyDate(lg.createdAt);
            if (nd && (!pd || nd > pd)) bestLogByInvoiceId.set(lg.invoiceId, lg);
        }

        // Join
        State.rowByLogId = {};
        const rows = [];

        for (const inv of invoices) {
            const lg = bestLogByInvoiceId.get(inv.id);
            if (!lg) continue;

            const parsed = safeJsonParse(lg.rawResponseJson) || {};
            const gibInvoiceNumber = parsed.invoiceNumber || parsed.InvoiceNumber || '';
            const gibId = parsed.id || parsed.Id || ''; // ETTN

            // UUID filtre (ETTN)
            if (ui.uuid) {
                if (!String(gibId || '').toLowerCase().includes(ui.uuid.toLowerCase())) continue;
            }

            const row = {
                logId: lg.id,
                invoiceId: inv.id,

                invoiceNumber: inv.invoiceNo,
                gibInvoiceNumber,
                gibId,

                targetTitle: inv.customerName,
                targetId: inv.taxNo,

                tipText: inv.typeText,
                issueDate: inv.invoiceDate,
                createdDate: lg.createdAt,

                kdv: inv.kdv,
                payable: inv.total,

                isAccount: inv.isAccountText || '-',
                emailStatus: inv.emailStatus || '-',

                resp: '',

                // status filtre için saklayalım
                portalStatus: null
            };

            State.rowByLogId[String(lg.id)] = row;
            rows.push(row);
        }

        rows.sort((a, b) => {
            const da = parseAnyDate(a.createdDate);
            const db = parseAnyDate(b.createdDate);
            return (db ? db.getTime() : 0) - (da ? da.getTime() : 0);
        });

        return rows;
    }

    // ---------------------------
    // Portal status hydrate
    // ---------------------------
    async function runPool(items, limit, worker) {
        let i = 0;
        const workers = Array.from({ length: Math.max(1, limit) }, () => (async () => {
            while (i < items.length) {
                const idx = i++;
                await worker(items[idx]);
            }
        })());
        await Promise.all(workers);
    }

    async function hydrateStatuses(items) {
        const rows = (items || []).filter(r => String(r.gibId || '').trim());
        if (!rows.length) return;

        await runPool(rows, 6, async (r) => {
            const ettN = String(r.gibId || '').trim();

            const url = buildGibPortalUrl(
                `TurkcellEFatura/earchive/status/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}`
            );

            try {
                const res = await fetchJsonPortal(url);

                if (!String(r.gibInvoiceNumber || '').trim() && res && res.invoiceNumber) {
                    r.gibInvoiceNumber = String(res.invoiceNumber || '');
                }

                r.portalStatus = (res && res.status !== undefined && res.status !== null) ? num(res.status) : null;
                r.resp = formatStatusResponse(res) || '';
            } catch (e) {
                r.portalStatus = null;
                r.resp = 'Durum alınamadı';
                console.error('[OutboxEarchiveInvoice] hydrateStatuses error:', e);
            }

            State.rowByLogId[String(r.logId)] = r;
        });
    }

    // ---------------------------
    // Table render
    // ---------------------------
    function setTotals(items) {
        let kdvTop = 0;
        let payTop = 0;

        for (const x of items) {
            kdvTop += num(x.kdv);
            payTop += num(x.payable);
        }

        $('#totalKDVTaxableAmount').text('0,00 TL');
        $('#totalKDVAmount').text(fmt(kdvTop) + ' TL');
        $('#totalPayableAmount').text(fmt(payTop) + ' TL');
    }

    function actionsHtml(r) {
        return `
          <div class="btn-group btn-group-xs">
            <button class="btn btn-default" title="PDF İndir" onclick="window.outboxDownloadPdf(${r.logId})">
              <i class="fa fa-download"></i>
            </button>
            <button class="btn btn-default" title="Durum Sorgula" onclick="window.outboxCheckStatus(${r.logId})">
              <i class="fa fa-info-circle"></i>
            </button>
          </div>
        `;
    }

    function renderTable(items) {
        setTotals(items);

        const hasDT = !!($.fn && $.fn.DataTable);

        if (hasDT) {
            if (State.dt) {
                State.dt.clear();
                State.dt.rows.add(items);
                State.dt.draw();
                $('#chkAll').prop('checked', false);
                return;
            }

            State.dt = $('#myDataTable').DataTable({
                data: items,
                destroy: true,
                paging: true,
                searching: true,
                order: [[6, 'desc']], // İşlem Tarihi
                columns: [
                    {
                        data: 'logId',
                        orderable: false,
                        render: (d) => `<input type="checkbox" class="rowchk" data-logid="${d}">`
                    },
                    {
                        // Fatura No/ETTN
                        data: null,
                        render: (r) => {
                            const gibNo = (r.gibInvoiceNumber || '').trim();
                            const gibId = (r.gibId || '').trim();
                            const fallback = (r.invoiceNumber || '').trim();

                            const main = gibNo || fallback || '';
                            const sub = gibId ? `ETTN: ${gibId}` : '';

                            return `${main}${sub ? `<br><small>${sub}</small>` : ''}`;
                        }
                    },
                    { data: 'targetTitle' },
                    { data: 'targetId' },
                    { data: 'tipText' },
                    {
                        data: 'issueDate',
                        render: (d) => {
                            const dt = parseAnyDate(d);
                            if (!dt) return '';
                            return `${pad2(dt.getDate())}.${pad2(dt.getMonth() + 1)}.${dt.getFullYear()}`;
                        }
                    },
                    {
                        data: 'createdDate',
                        render: (d) => {
                            const dt = parseAnyDate(d);
                            if (!dt) return (d ? String(d).replace('T', ' ').substring(0, 16) : '');
                            return `${pad2(dt.getDate())}.${pad2(dt.getMonth() + 1)}.${dt.getFullYear()} ${pad2(dt.getHours())}:${pad2(dt.getMinutes())}`;
                        }
                    },
                    { data: 'kdv', className: 'text-right', render: (d) => fmt(d) },
                    { data: 'payable', className: 'text-right', render: (d) => fmt(d) },
                    { data: 'isAccount', className: 'text-center', render: (d) => (d || '-') },
                    { data: 'resp', className: 'text-center', render: (d) => (d || '') },      // Durum (Gib/Cevap)
                    { data: 'emailStatus', className: 'text-center', render: (d) => (d || '-') }, // E-Posta Durumu
                    { data: null, orderable: false, className: 'text-center', render: (r) => actionsHtml(r) }
                ],
                drawCallback: function () { $('#chkAll').prop('checked', false); }
            });

            $('#chkAll').off('change').on('change', function () {
                const c = $(this).is(':checked');
                $('#myDataTable .rowchk').prop('checked', c);
            });

            return;
        }

        // DataTables yoksa
        const $tb = $('#myDataTable tbody');
        $tb.empty();

        for (const r of items) {
            const dtInv = parseAnyDate(r.issueDate);
            const dtLog = parseAnyDate(r.createdDate);

            const invDateTxt = dtInv ? `${pad2(dtInv.getDate())}.${pad2(dtInv.getMonth() + 1)}.${dtInv.getFullYear()}` : '';
            const logDateTxt = dtLog ? `${pad2(dtLog.getDate())}.${pad2(dtLog.getMonth() + 1)}.${dtLog.getFullYear()} ${pad2(dtLog.getHours())}:${pad2(dtLog.getMinutes())}` : '';

            const main = (r.gibInvoiceNumber || '').trim() || (r.invoiceNumber || '').trim();
            const sub = r.gibId ? `ETTN: ${r.gibId}` : '';

            $tb.append(`
                <tr>
                    <td><input type="checkbox" class="rowchk" data-logid="${r.logId}"></td>
                    <td>${main || ''}${sub ? `<br><small>${sub}</small>` : ''}</td>
                    <td>${r.targetTitle || ''}</td>
                    <td>${r.targetId || ''}</td>
                    <td>${r.tipText || ''}</td>
                    <td>${invDateTxt}</td>
                    <td>${logDateTxt}</td>
                    <td class="text-right">${fmt(r.kdv)}</td>
                    <td class="text-right">${fmt(r.payable)}</td>
                    <td class="text-center">${r.isAccount || '-'}</td>
                    <td class="text-center">${r.resp || ''}</td>
                    <td class="text-center">${r.emailStatus || '-'}</td>
                    <td class="text-center">${actionsHtml(r)}</td>
                </tr>
            `);
        }

        $('#chkAll').off('change').on('change', function () {
            const c = $(this).is(':checked');
            $('#myDataTable .rowchk').prop('checked', c);
        });
    }

    function applyStatusFilter(items, statusFilter) {
        const f = String(statusFilter || '').trim();
        if (!f) return items;

        if (f === 'Hata') {
            // Hata = portalStatus 40 veya metinde "Hata"
            return (items || []).filter(x =>
                num(x.portalStatus) === 40 || String(x.resp || '').toLowerCase().includes('hata')
            );
        }
        if (f === 'Hatasiz') {
            // Hatasız = portalStatus var ve 40 değil (taslak dahil)
            return (items || []).filter(x =>
                x.portalStatus !== null && num(x.portalStatus) !== 40
            );
        }

        return items;
    }

    // ---------------------------
    // Main load
    // ---------------------------
    async function loadList() {
        try {
            const ui = buildParamsFromUI();

            // 1) ERP’den çek + join
            let rows = await loadDataAndJoin(ui);

            // 2) Portal status ile doldur (Durum kolonu için şart)
            await hydrateStatuses(rows);

            // 3) Status filtresi varsa uygula
            rows = applyStatusFilter(rows, ui.statusFilter);

            // 4) State + render
            State.items = rows;
            renderTable(State.items);

        } catch (e) {
            console.error('[OutboxEarchiveInvoice] loadList error:', e);
            err((e && e.message) ? e.message : 'Liste alınamadı.');
            State.items = [];
            renderTable([]);
        }
    }

    // ---------------------------
    // Datepicker init
    // ---------------------------
    function initDatepicker() {
        if (!$ || !$.fn) return;

        const months = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
        const monthsShort = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
        const days = ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"];
        const daysShort = ["Paz", "Pts", "Sal", "Çar", "Per", "Cum", "Cts"];
        const daysMin = ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"];

        if ($.datepicker && typeof $.datepicker.setDefaults === "function") {
            $.datepicker.setDefaults({
                closeText: "Kapat", prevText: "Önceki", nextText: "Sonraki", currentText: "Bugün",
                monthNames: months, monthNamesShort: monthsShort,
                dayNames: days, dayNamesShort: daysShort, dayNamesMin: daysMin,
                dateFormat: "dd.mm.yy", firstDay: 1, isRTL: false
            });

            $('.datepicker').datepicker({
                changeMonth: true,
                changeYear: true,
                showAnim: "fadeIn",
                dateFormat: "dd.mm.yy"
            });
            return;
        }

        if ($.fn.datepicker && $.fn.datepicker.dates) {
            $.fn.datepicker.dates.tr = {
                days, daysShort, daysMin,
                months, monthsShort,
                today: "Bugün",
                clear: "Temizle",
                weekStart: 1,
                format: "dd.mm.yyyy"
            };

            $('.datepicker').datepicker({
                language: 'tr',
                format: 'dd.mm.yyyy',
                autoclose: true,
                todayHighlight: true
            });
            return;
        }

        console.warn("[OutboxEarchiveInvoice] Datepicker bulunamadı.");
    }

    function clearSearchInputs() {
        $('#IssueDateStart,#IssueDateStartEnd').val('');
        $('#DocumentId,#UUID,#TargetIdentifier,#TargetTitle').val('');
        $('#Status').val('');
        $('#TargetIdentifierChk').prop('checked', false);

        setDefaultMonthDatesIfEmpty();
        loadList();
    }

    // ---------------------------
    // Exposed actions
    // ---------------------------

    // PDF indir: Durum alanına dokunmaz
    global.outboxDownloadPdf = (logId) => {
        const row = State.rowByLogId[String(logId)];
        if (!row) return err('Kayıt bulunamadı.');

        const ettN = String(row.gibId || '').trim();
        if (!ettN) return err('ETTN (rawResponseJson.id) bulunamadı.');

        const url = buildGibPortalUrl(
            `TurkcellEFatura/earchive/pdf/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}&standardXslt=true`
        );

        global.open(url, '_blank', 'noopener');
    };

    global.outboxCheckStatus = async (logId) => {
        const row = State.rowByLogId[String(logId)];
        if (!row) return err('Kayıt bulunamadı.');

        const ettN = String(row.gibId || '').trim();
        if (!ettN) return err('ETTN (rawResponseJson.id) bulunamadı.');

        const url = buildGibPortalUrl(
            `TurkcellEFatura/earchive/status/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}`
        );

        try {
            const res = await fetchJsonPortal(url);

            if (!String(row.gibInvoiceNumber || '').trim() && res && res.invoiceNumber) {
                row.gibInvoiceNumber = String(res.invoiceNumber || '');
            }

            row.portalStatus = (res && res.status !== undefined && res.status !== null) ? num(res.status) : null;
            row.resp = formatStatusResponse(res) || '';

            State.rowByLogId[String(row.logId)] = row;

            const idx = State.items.findIndex(x => String(x.logId) === String(row.logId));
            if (idx >= 0) State.items[idx] = row;

            renderTable(State.items);
        } catch (e) {
            console.error('[OutboxEarchiveInvoice] outboxCheckStatus error:', e);
            err((e && e.message) ? e.message : 'Durum alınamadı.');
        }
    };

    // ---------------------------
    // Init
    // ---------------------------
    function init() {
        if (!$) {
            console.error('[OutboxEarchiveInvoice] jQuery yok.');
            return;
        }

        try {
            State.userId = getCurrentUserId();
        } catch (e) {
            console.error(e);
            err(e.message);
            return;
        }

        initDatepicker();
        setDefaultMonthDatesIfEmpty();

        $('#btnSearch').off('click').on('click', loadList);
        $('#btnClear').off('click').on('click', clearSearchInputs);

        $('#btnAramaYapEnter').off('keypress').on('keypress', 'input', function (e) {
            if (e.which === 13) { e.preventDefault(); loadList(); }
        });

        $('#btnDetayliArama').off('click').on('click', function () {
            const $i = $(this).find('.myCollapseIcon');
            setTimeout(() => $i.toggleClass('fa-plus fa-minus'), 150);
        });

        $('.dropdown-menu').off('click.outbox').on('click.outbox', 'a.bulk', function (e) {
            e.preventDefault();
            err('Bu ekranda toplu işlem/senkron aksiyonları kullanılmıyor.');
        });

        loadList();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})(window);
