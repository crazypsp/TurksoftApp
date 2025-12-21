// wwwroot/apps/OutboxInvoice.js
// NOT: Bu dosya artık ../Base/turkcellEfaturaApi.js import etmez.
// İki endpoint’ten (GibInvoice + GibGibInvoiceOperationLog) verileri çekip,
// Invoice.UserId + InvoiceDate tarih aralığına göre filtreler,
// OperationLog (OperationName=SendEInvoiceJson, ErrorCode=200) ile Invoice.Id eşleştirip
// myDataTable’a basar.

(function (global) {
    'use strict';

    const $ = global.jQuery;

    // ---------------------------
    // UI helpers
    // ---------------------------
    function ok(m) { if (global.toastr?.success) global.toastr.success(m); else alert(m); }
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
        if (yy.length === 2) yy = '20' + yy; // 25 -> 2025 varsayımı
        if (yy.length !== 4) return '';
        return `${yy}-${mm}-${dd}`;
    }

    // ISO veya "yyyy-MM-dd HH:mm:ss.fffffff" gibi .NET formatlarını güvenli parse eder.
    function parseAnyDate(val) {
        if (!val) return null;
        const s = String(val).trim();

        // ISO ise
        let t = Date.parse(s);
        if (Number.isFinite(t)) return new Date(t);

        // .NET: /Date(1700000000000)/
        const mDotNet = s.match(/Date\((\d+)\)/i);
        if (mDotNet) {
            const ms = parseInt(mDotNet[1], 10);
            if (!isNaN(ms)) return new Date(ms);
        }

        // "yyyy-MM-dd HH:mm:ss.fffffff" veya "yyyy-MM-ddTHH:mm:ss"
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

            // 7 hane gelebiliyor; ilk 3 haneyi ms olarak al
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
    // API helpers
    // ---------------------------
    function normalizeBaseUrl() {
        let base = String(global.__API_BASE || '').trim();
        base = base.replace(/\/+$/, ''); // trailing slash temizle
        return base;
    }

    // window.__API_BASE:
    //  - "https://giberp.noxmusavir.com"  -> /api/v1/<resource>
    //  - "https://giberp.noxmusavir.com/api/v1" -> /<resource>
    //  - "/api/v1" -> /<resource>
    function buildApiUrl(resource) {
        const base = normalizeBaseUrl();

        // base yoksa aynı origin varsay
        if (!base) return `/api/v1/${resource}`;

        // base zaten /api/v1 ile bitiyorsa tekrar ekleme
        if (/\/api\/v1$/i.test(base)) return `${base}/${resource}`;

        return `${base}/api/v1/${resource}`;
    }

    async function fetchJson(url) {
        const res = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            credentials: 'include'
        });

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        // bazı ortamlarda boş dönebiliyor
        const text = await res.text();
        if (!text) return null;

        try { return JSON.parse(text); } catch { return text; }
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
        const userId = num(pick(inv, ['UserId', 'userId']));                // ✅ Invoice.UserId
        const customerId = num(pick(inv, ['CustomerId', 'customerId']));

        const invoiceNo = String(pick(inv, ['InvoiceNo', 'invoiceNo'], '') || '');
        const invoiceDate = pick(inv, ['InvoiceDate', 'invoiceDate'], '');

        const total = num(pick(inv, ['Total', 'total'], 0));
        const currency = String(pick(inv, ['Currency', 'currency'], '') || '');

        const type = pick(inv, ['Type', 'type'], '');
        const typeText = (type === 0 || type === '0') ? 'SATIŞ' : String(type ?? '');

        // müşteri bilgileri
        const c = inv.customer || inv.Customer || null;
        const customerName = c ? `${c.name || ''} ${c.surname || ''}`.trim() : '';
        const taxNo = c ? (c.taxNo || c.taxno || '') : '';

        // KDV (InvoicesTaxes)
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
            fetchJson(invoiceUrl),
            fetchJson(logUrl)
        ]);

        const invAll = Array.isArray(invRes) ? invRes : (invRes?.items || []);
        const logAll = Array.isArray(logRes) ? logRes : (logRes?.items || []);

        const invoicesAll = (invAll || []).map(mapInvoice);

        // ✅ Invoice filtre: Invoice.UserId + InvoiceDate aralığı
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

        // Log filtre + aynı InvoiceId için en güncel log’u seç
        const bestLogByInvoiceId = new Map();

        for (const r of (logAll || [])) {
            const lg = mapLog(r);
            if (!lg.invoiceId) continue;

            // ✅ Şartlar: OperationName=SendEInvoiceJson ve ErrorCode=200
            if (lg.operationName !== 'SendEInvoiceJson') continue;
            if (String(lg.errorCode ?? '').trim() !== '200') continue;

            // yalnızca filtrelenmiş invoice seti
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

        // Join: sadece başarılı log’u olan invoice’ları listele
        State.rowByLogId = {};
        const rows = [];

        for (const inv of invoices) {
            const lg = bestLogByInvoiceId.get(inv.id);
            if (!lg) continue;

            const parsed = safeJsonParse(lg.rawResponseJson) || {};
            const gibInvoiceNumber = parsed.invoiceNumber || parsed.InvoiceNumber || '';
            const gibId = parsed.id || parsed.Id || '';

            const statusText = `200 / ${lg.isSuccess ? 'Başarılı' : 'Başarısız'}`;

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
                status: statusText,
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
            <button class="btn btn-default" title="RawResponseJson" onclick="window.outboxShowRaw(${r.logId})">
              <i class="fa fa-code"></i>
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
                        data: null,
                        render: (r) => {
                            const invNo = r.invoiceNumber || '';
                            const gibNo = r.gibInvoiceNumber || '';
                            const gibId = r.gibId || '';
                            const sub = [gibNo ? `GİB No: ${gibNo}` : '', gibId ? `GİB Id: ${gibId}` : ''].filter(Boolean).join(' | ');
                            return `${invNo}${sub ? `<br><small>${sub}</small>` : ''}`;
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
                    {
                        data: null,
                        className: 'text-center',
                        render: (r) => {
                            const s = r.status || '';
                            const a = r.resp || '';
                            return s + (a ? ` / ${a}` : '');
                        }
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

        // DataTables yoksa düz render
        const $tb = $('#myDataTable tbody');
        $tb.empty();

        for (const r of items) {
            const dtInv = parseAnyDate(r.issueDate);
            const dtLog = parseAnyDate(r.createdDate);

            const invDateTxt = dtInv ? `${pad2(dtInv.getDate())}.${pad2(dtInv.getMonth() + 1)}.${dtInv.getFullYear()}` : '';
            const logDateTxt = dtLog ? `${pad2(dtLog.getDate())}.${pad2(dtLog.getMonth() + 1)}.${dtLog.getFullYear()} ${pad2(dtLog.getHours())}:${pad2(dtLog.getMinutes())}` : '';

            const sub = [
                r.gibInvoiceNumber ? `GİB No: ${r.gibInvoiceNumber}` : '',
                r.gibId ? `GİB Id: ${r.gibId}` : ''
            ].filter(Boolean).join(' | ');

            $tb.append(`
                <tr>
                    <td><input type="checkbox" class="rowchk" data-logid="${r.logId}"></td>
                    <td>${r.invoiceNumber || ''}${sub ? `<br><small>${sub}</small>` : ''}</td>
                    <td>${r.targetTitle || ''}</td>
                    <td>${r.targetId || ''}</td>
                    <td>${r.tipText || ''}</td>
                    <td>${invDateTxt}</td>
                    <td>${logDateTxt}</td>
                    <td class="text-right">${fmt(r.kdv)}</td>
                    <td class="text-right">${fmt(r.payable)}</td>
                    <td class="text-center">${r.isAccount}</td>
                    <td class="text-center">${(r.status || '')}</td>
                    <td class="text-center">${actionsHtml(r)}</td>
                </tr>
            `);
        }

        $('#chkAll').off('change').on('change', function () {
            const c = $(this).is(':checked');
            $('#myDataTable .rowchk').prop('checked', c);
        });
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

        // 1) jQuery UI Datepicker
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

        // 2) bootstrap-datepicker
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
        // Bu ekranda sadece tarih + documentId filtreleniyor (diğer alanlar şimdilik pasif)
        $('#IssueDateStart,#IssueDateStartEnd').val('');
        $('#DocumentId').val('');

        // tekrar default ayı set edip yükle
        setDefaultMonthDatesIfEmpty();
        loadList();
    }

    // ---------------------------
    // Exposed actions
    // ---------------------------
    global.outboxShowRaw = (logId) => {
        const row = State.rowByLogId[String(logId)];
        if (!row) return err('Kayıt bulunamadı.');

        const parsed = safeJsonParse(row.rawResponseJson);
        const content = parsed ? JSON.stringify(parsed, null, 2) : String(row.rawResponseJson || '');

        // modal varsa onu kullan, yoksa alert
        const $m = $('#modal-raw');
        const $ta = $('#rawJsonText');
        if ($m.length && $ta.length) {
            $ta.val(content);
            $m.modal('show');
        } else {
            alert(content);
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

        // Search / Clear
        $('#btnSearch').off('click').on('click', loadList);
        $('#btnClear').off('click').on('click', clearSearchInputs);

        // Enter ile arama (IssueDateStart/End ve DocumentId gibi inputlar)
        $('#btnAramaYapEnter').off('keypress').on('keypress', 'input', function (e) {
            if (e.which === 13) { e.preventDefault(); loadList(); }
        });

        // Detaylı Arama ikon
        $('#btnDetayliArama').off('click').on('click', function () {
            const $i = $(this).find('.myCollapseIcon');
            setTimeout(() => $i.toggleClass('fa-plus fa-minus'), 150);
        });

        // Bulk menü tıklanırsa şimdilik pasif (hata vermesin)
        $('.dropdown-menu').off('click.outbox').on('click.outbox', 'a.bulk', function (e) {
            e.preventDefault();
            err('Bu ekranda toplu işlem/senkron aksiyonları kullanılmıyor.');
        });

        // İlk liste
        loadList();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})(window);
