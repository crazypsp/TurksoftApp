// wwwroot/apps/OutboxInvoice.js
// NOT: Bu dosya artık ../Base/turkcellEfaturaApi.js import etmez.
// İki endpoint’ten (GibInvoice + GibGibInvoiceOperationLog) verileri çekip,
// Invoice.UserId + InvoiceDate tarih aralığına göre filtreler,
// OperationLog (OperationName=SendEInvoiceJson, ErrorCode=200) ile Invoice.Id eşleştirip
// myDataTable’a basar.
// Ayrıca gibPortalApiBaseUrl üzerinden status endpoint'i çağırıp "Gib/Cevap" alanını doldurur.

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
            credentials: o.credentials || 'include', // ERP tarafı genelde cookie ister
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
        // Layout: window.gibPortalApiBaseUrl = '@ApiSettings.Value.GibPortalApiBaseUrl';
        // Boşsa aynı origin'de /api varsay.
        let base = String(global.gibPortalApiBaseUrl || '').trim();
        if (!base) base = `${global.location.origin}/api`;

        base = base.replace(/\/+$/, '');

        // Her durumda /api ile bitsin (örnek: https://localhost:7151/api)
        if (!/\/api$/i.test(base)) base = `${base}/api`;

        return base;
    }

    // pathAfterApi: "TurkcellEFatura/einvoice/outbox/status/<id>?userId=11"
    // => https://.../api/TurkcellEFatura/...
    function buildGibPortalUrl(pathAfterApi) {
        const base = normalizeGibPortalBaseUrl();
        let p = String(pathAfterApi || '').trim();
        p = p.replace(/^\/+/, '');
        p = p.replace(/^api\//i, ''); // yanlışlıkla api/ ile gelirse kırp
        return `${base}/${p}`;
    }

    async function fetchJsonPortal(url) {
        // Portal endpoint'i çoğunlukla cookie istemez, cross-origin ise include CORS’u patlatır.
        // Bu yüzden omit.
        try {
            return await fetchJson(url, { credentials: 'omit', mode: 'cors' });
        } catch (e) {
            // Failed to fetch -> kullanıcıya daha anlamlı hata
            if (e && String(e).includes('TypeError')) {
                throw new Error(`İstek atılamadı (Failed to fetch). URL: ${url}. Muhtemel sebep: CORS/SSL/Network.`);
            }
            throw e;
        }
    }

    // ---------------------------
    // Status mappings (Gib Portal)
    // ---------------------------
    const InvoiceStatusText = {
        0: 'Taslak',
        20: 'Kuyruk',
        30: "Gib'e Gönderiliyor",
        40: 'Hata',
        50: "Gib'e İletildi",
        60: 'Onaylandı',
        61: 'Onaylanıyor',
        62: 'Onaylama Hatası',
        65: 'Otomatik Onaylandı',
        70: 'Onay Bekliyor',
        80: 'Reddedildi',
        81: 'Reddediliyor',
        82: 'Reddetme Hatası',
        99: 'e-Fatura İptal'
    };

    const EnvelopeStatusText = {
        1000: 'Zarf kuyruğa Ekledi',
        1100: 'Zarf İşleniyor',
        1120: 'Zarf Arşivden Kopyalanamadı',
        1110: 'Zip Dosyası Değil',
        1111: 'Zarf Id Uzunluğu Geçersiz',
        1130: 'Zip Açılamadı',
        1131: 'Zip Bir Dosya İçermeli',
        1132: 'Xml Dosyası Değil',
        1133: 'Zarf Id ve Xml Dosyasının Adı Aynı Olmalı',
        1140: 'Döküman Ayrıştırılamadı',
        1141: 'Zarf Id Yok',
        1143: 'Geçersiz Versiyon',
        1150: 'Schematron Kontrol Sonucu Hatalı',
        1160: 'Xml Şema Kontrolünden Geçemedi',
        1161: 'İmza Sahibi Tckn Vkn Alınamadı',
        1162: 'İmza Kaydedilemedi',
        1163: 'Gönderilen Zarf Sistemde Daha Önce Kayıtlı Olan Bir Faturayı İçermektedir',
        1170: 'Yetki Kontrol Edilemedi',
        1171: 'Gönderici Birim Yetkisi Yok',
        1172: 'Posta Kutusu Yetkisi Yok',
        1175: 'İmza Yetkisi Kontrol Edilemedi',
        1176: 'İmza Sahibi Yetkisiz',
        1177: 'Geçersiz İmza',
        1180: 'Adres Kontrol Edilemedi',
        1181: 'Adres Bulunamadı',
        1182: 'Kullanıcı Eklenemedi',
        1183: 'Kullanıcı Silinemedi',
        1190: 'Sistem Yanıtı Hazırlanamadı',
        1195: 'Sistem Hatası',
        1200: 'Zarf Başarıyla İşlendi',
        1210: 'Döküman Bulunan Adrese Gönderilemedi',
        1215: 'Döküman Gönderimi Başarısız',
        1220: 'Hedeften Sistem Yanıtı Gelmedi',
        1230: 'Hedeften Sistem Yanıtı Başarısız Geldi',
        1235: 'Fatura İptale Konu Edildi',
        1300: 'Başarıyla Tamamlandı'
    };

    function formatStatusResponse(r) {
        if (!r || typeof r !== 'object') return '';

        const st = (r.status !== undefined && r.status !== null) ? num(r.status) : null;
        if (st === 0) return 'Taslak'; // kullanıcı isteği: status=0 -> sadece Taslak

        const es = (r.envelopeStatus !== undefined && r.envelopeStatus !== null) ? num(r.envelopeStatus) : null;

        const stTxt = (st !== null) ? (InvoiceStatusText[st] || 'Bilinmeyen') : '—';
        const esTxt = (es !== null) ? (EnvelopeStatusText[es] || 'Bilinmeyen') : '—';

        const m1 = String(r.message || '').trim();
        const m2 = String(r.envelopeMessage || '').trim();

        let msg = '';
        if (m1 && m2 && m1 !== m2) msg = `${m1} | ${m2}`;
        else msg = (m1 || m2 || '');

        const left = `${st !== null ? st : '—'}(${stTxt}) / ${es !== null ? es : '—'}(${esTxt})`;
        return msg ? `${left} - ${msg}` : left;
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
        const customerId = num(pick(inv, ['CustomerId', 'customerId']));

        const invoiceNo = String(pick(inv, ['InvoiceNo', 'invoiceNo'], '') || '');
        const invoiceDate = pick(inv, ['InvoiceDate', 'invoiceDate'], '');

        const total = num(pick(inv, ['Total', 'total'], 0));
        const currency = String(pick(inv, ['Currency', 'currency'], '') || '');

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

        return {
            id,
            userId,
            customerId,
            invoiceNo,
            invoiceDate,
            total,
            currency,
            typeText,
            customerName,
            taxNo,
            kdv,
            raw: inv
        };
    }

    function mapLog(logRaw) {
        const lg = logRaw || {};
        return {
            id: num(pick(lg, ['Id', 'id'])),
            invoiceId: num(pick(lg, ['InvoiceId', 'invoiceId'])),
            operationName: String(pick(lg, ['OperationName', 'operationName'], '') || ''),
            isSuccess: !!pick(lg, ['IsSuccess', 'isSuccess'], false),
            errorCode: pick(lg, ['ErrorCode', 'errorCode'], ''),
            errorMessage: String(pick(lg, ['ErrorMessage', 'errorMessage'], '') || ''),
            rawResponseJson: pick(lg, ['RawResponseJson', 'rawResponseJson'], ''),
            createdAt: pick(lg, ['CreatedAt', 'createdAt'], ''),
            raw: lg
        };
    }

    // ---------------------------
    // UI params + default date range (aktif ay)
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

    function buildParamsFromUI() {
        setDefaultMonthDatesIfEmpty();

        const startIso = toIsoDateTR($('#IssueDateStart').val());
        const endIso = toIsoDateTR($('#IssueDateStartEnd').val());

        return {
            userId: State.userId,
            invoiceDateStartIso: startIso,
            invoiceDateEndIso: endIso,
            documentId: String($('#DocumentId').val() || '').trim()
        };
    }

    // ---------------------------
    // Load + join
    // ---------------------------
    async function loadDataAndJoin(ui) {
        const invoiceUrl = buildApiUrl('GibInvoice');
        const logUrl = buildApiUrl('GibGibInvoiceOperationLog');

        const [invRes, logRes] = await Promise.all([
            fetchJson(invoiceUrl, { credentials: 'include' }),
            fetchJson(logUrl, { credentials: 'include' })
        ]);

        const invAll = Array.isArray(invRes) ? invRes : (invRes?.items || []);
        const logAll = Array.isArray(logRes) ? logRes : (logRes?.items || []);

        const invoicesAll = (invAll || []).map(mapInvoice);

        const invoices = invoicesAll.filter(inv => {
            if (num(inv.userId) !== num(ui.userId)) return false;

            const d = parseAnyDate(inv.invoiceDate);
            if (!inRange(d, ui.invoiceDateStartIso, ui.invoiceDateEndIso)) return false;

            if (ui.documentId) {
                if (!String(inv.invoiceNo || '').toLowerCase().includes(ui.documentId.toLowerCase())) return false;
            }

            return true;
        });

        const invById = new Map(invoices.map(x => [x.id, x]));

        const bestLogByInvoiceId = new Map();

        for (const r of (logAll || [])) {
            const lg = mapLog(r);
            if (!lg.invoiceId) continue;

            if (lg.operationName !== 'SendEInvoiceJson') continue;
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

        State.rowByLogId = {};
        const rows = [];

        for (const inv of invoices) {
            const lg = bestLogByInvoiceId.get(inv.id);
            if (!lg) continue;

            const parsed = safeJsonParse(lg.rawResponseJson) || {};
            const gibInvoiceNumber = parsed.invoiceNumber || parsed.InvoiceNumber || '';
            const gibId = parsed.id || parsed.Id || ''; // ETTN

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

                isAccount: '-',

                // Artık bu kolonda sadece portal status bilgisini göstereceğiz.
                // Bu yüzden status alanını boş bırakıyoruz, asıl metin resp'te olacak.
                status: '',
                resp: '',

                rawResponseJson: lg.rawResponseJson,
                rawInvoice: inv.raw,
                rawLog: lg.raw
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
    // Status prefill (tüm satırlar)
    // ---------------------------
    async function runPool(items, limit, worker) {
        let i = 0;
        const workers = Array.from({ length: Math.max(1, limit) }, () => (async () => {
            while (i < items.length) {
                const idx = i++;
                const it = items[idx];
                await worker(it);
            }
        })());
        await Promise.all(workers);
    }

    async function hydrateStatuses(items) {
        const rows = (items || []).filter(r => String(r.gibId || '').trim());
        if (!rows.length) return;

        // İstersen burada geçici "Yükleniyor..." yazdırabilirsin; sen istemediğin için boş bırakıyorum.

        await runPool(rows, 6, async (r) => {
            const key = String(r.logId);
            const ettN = String(r.gibId || '').trim();

            const url = buildGibPortalUrl(
                `TurkcellEFatura/einvoice/outbox/status/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}`
            );

            try {
                const res = await fetchJsonPortal(url);

                // Eğer rawResponseJson'da invoiceNumber gelmediyse status response'tan tamamla
                if (!String(r.gibInvoiceNumber || '').trim() && res && res.invoiceNumber) {
                    r.gibInvoiceNumber = String(res.invoiceNumber || '');
                }

                r.resp = formatStatusResponse(res) || '';
            } catch (e) {
                // fetch fail / CORS vb.
                r.resp = 'Durum alınamadı';
                console.error('[OutboxInvoice] hydrateStatuses error:', e);
            }

            State.rowByLogId[key] = r;

            const idx = State.items.findIndex(x => String(x.logId) === key);
            if (idx >= 0) State.items[idx] = r;

            refreshRow(r.logId);
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
                order: [[6, 'desc']],
                columns: [
                    {
                        data: 'logId',
                        orderable: false,
                        render: (d) => `<input type="checkbox" class="rowchk" data-logid="${d}">`
                    },
                    {
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
                    { data: 'isAccount', className: 'text-center' },

                    // ✅ Bu kolon artık sadece Gib/Cevap (status endpoint sonucu)
                    {
                        data: null,
                        className: 'text-center',
                        render: (r) => (r.resp || '')
                    },

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
                    <td class="text-center">${r.isAccount}</td>
                    <td class="text-center">${r.resp || ''}</td>
                    <td class="text-center">${actionsHtml(r)}</td>
                </tr>
            `);
        }

        $('#chkAll').off('change').on('change', function () {
            const c = $(this).is(':checked');
            $('#myDataTable .rowchk').prop('checked', c);
        });
    }

    function refreshRow(logId) {
        if (State.dt) {
            const key = String(logId);
            const rowData = State.rowByLogId[key];
            if (!rowData) return;

            State.dt.rows().every(function () {
                const d = this.data();
                if (d && String(d.logId) === key) {
                    this.data(rowData);
                }
            });
            State.dt.draw(false);
            return;
        }

        renderTable(State.items);
    }

    // ---------------------------
    // Main load
    // ---------------------------
    async function loadList() {
        try {
            const ui = buildParamsFromUI();
            const rows = await loadDataAndJoin(ui);

            State.items = rows;
            renderTable(State.items);

            // ✅ tüm satırların Gib/Cevap'ını status endpoint'ten doldur
            await hydrateStatuses(State.items);

        } catch (e) {
            console.error('[OutboxInvoice] loadList error:', e);
            err((e && e.message) ? e.message : 'Liste alınamadı.');
            renderTable([]);
        }
    }

    // ---------------------------
    // Datepicker init (jQuery UI / bootstrap-datepicker uyumlu)
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

        console.warn("[OutboxInvoice] Datepicker bulunamadı. (jQuery UI veya bootstrap-datepicker yüklenmeli)");
    }

    function clearSearchInputs() {
        $('#IssueDateStart,#IssueDateStartEnd').val('');
        $('#DocumentId').val('');

        setDefaultMonthDatesIfEmpty();
        loadList();
    }

    // ---------------------------
    // Exposed actions
    // ---------------------------

    // ✅ PDF indir: Gib/Cevap alanına hiçbir şey yazmaz.
    global.outboxDownloadPdf = (logId) => {
        const row = State.rowByLogId[String(logId)];
        if (!row) return err('Kayıt bulunamadı.');

        const ettN = String(row.gibId || '').trim();
        if (!ettN) return err('ETTN (rawResponseJson.id) bulunamadı.');

        const url = buildGibPortalUrl(
            `TurkcellEFatura/einvoice/outbox/pdf/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}&standardXslt=true`
        );

        // sadece indir
        global.open(url, '_blank', 'noopener');
    };

    // ✅ Tek satır status sorgula (isteğe bağlı)
    global.outboxCheckStatus = async (logId) => {
        const key = String(logId);
        const row = State.rowByLogId[key];
        if (!row) return err('Kayıt bulunamadı.');

        const ettN = String(row.gibId || '').trim();
        if (!ettN) return err('ETTN (rawResponseJson.id) bulunamadı.');

        const url = buildGibPortalUrl(
            `TurkcellEFatura/einvoice/outbox/status/${encodeURIComponent(ettN)}?userId=${encodeURIComponent(State.userId)}`
        );

        try {
            const res = await fetchJsonPortal(url);

            if (!String(row.gibInvoiceNumber || '').trim() && res && res.invoiceNumber) {
                row.gibInvoiceNumber = String(res.invoiceNumber || '');
            }

            row.resp = formatStatusResponse(res) || '';
            State.rowByLogId[key] = row;

            const idx = State.items.findIndex(x => String(x.logId) === key);
            if (idx >= 0) State.items[idx] = row;

            refreshRow(logId);
        } catch (e) {
            console.error('[OutboxInvoice] outboxCheckStatus error:', e);
            row.resp = 'Durum alınamadı';
            State.rowByLogId[key] = row;

            const idx = State.items.findIndex(x => String(x.logId) === key);
            if (idx >= 0) State.items[idx] = row;

            refreshRow(logId);
            err((e && e.message) ? e.message : 'Durum alınamadı.');
        }
    };

    // ---------------------------
    // Init
    // ---------------------------
    function init() {
        if (!$) {
            console.error('[OutboxInvoice] jQuery yok.');
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
