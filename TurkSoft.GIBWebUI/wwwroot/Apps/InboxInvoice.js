// wwwroot/apps/einvoice/InboxInvoice.js
import { EinvoiceInboxApi } from '../Base/turkcellEfaturaApi.js';

(function (global) {
    'use strict';

    const $ = global.jQuery;

    function toastOk(m) { if (global.toastr?.success) toastr.success(m); else alert(m); }
    function toastErr(m) { if (global.toastr?.error) toastr.error(m); else alert(m); }

    function num(v) {
        const n = Number(v);
        return Number.isFinite(n) ? n : 0;
    }

    function fmtMoneyTR(n) {
        return (Number(n) || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // datetime-local => "yyyy-MM-dd HH:mm:ss"
    function normalizeHtmlDateTime(v, isEnd) {
        if (!v) return '';
        if (v.includes('T')) {
            const [d, t] = v.split('T');
            const tt = (t && t.length === 5) ? `${t}:00` : (t || '00:00:00');
            return `${d} ${tt}`;
        }
        return v + (isEnd ? ' 23:59:59' : ' 00:00:00');
    }

    function getCurrentUserIdForGib() {
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

    // ===== EK: UI code -> integer status (API uyumu) =====
    function mapUiResponseCodeToStatusInt(code) {
        if (code === undefined || code === null) return null;
        const s = String(code).trim();
        if (!s) return null;

        // "0", "1" gibi numeric ise direkt int
        if (/^\d+$/.test(s)) return parseInt(s, 10);

        const u = s.toUpperCase();
        if (u === 'KABUL' || u === 'ACCEPT' || u === 'APPROVE') return 1;
        if (u === 'RED' || u === 'REJECT' || u === 'DECLINE') return 2;

        return null;
    }

    // ===== EK: Base service error.response içinden validation messages çıkar =====
    function extractApiErrorMessage(e) {
        const r = e?.response;
        if (r && typeof r === 'object') {
            if (r.errors && typeof r.errors === 'object') {
                const msgs = [];
                Object.keys(r.errors).forEach(k => {
                    const arr = r.errors[k];
                    if (Array.isArray(arr)) arr.forEach(m => {
                        const s = String(m || '').trim();
                        if (s) msgs.push(s);
                    });
                });
                if (msgs.length) return msgs.join('\n');
            }
            return r.detail || r.title || r.message || e?.message;
        }
        return e?.message || 'Uygulama yanıtı gönderilemedi.';
    }

    // View’deki onclick’ler bozulmasın diye global fonksiyonları sağlayacağız
    const State = {
        userId: 0,
        items: [],
        dt: null,   // DataTables varsa
    };

    function mapRow(raw) {
        const r = raw || {};

        const uuid =
            r.uuid || r.UUID ||  r.id || r.Id || '';

        const invoiceNumber = r.invoiceNumber || r.documentId || r.DocumentId || r.invoiceNo || '';
        const senderTitle = r.targetTitle || r.senderTitle || r.companyTitle || '';
        const senderVkn = r.targetVknTckn || r.senderVknTckn || r.senderIdentifier || '';

        const scenario = r.profileId || r.scenario || r.Scenario || '';
        const type = r.invoiceTypeCode || r.type || r.InvoiceTypeCode || '';
        const tipText = (type && scenario) ? `${type} / ${scenario}` : (type || scenario || '');

        const issueDate = r.issueDate || r.executionDate || r.InvoiceDate || '';
        const createdDate = r.createdDate || r.integrationDate || r.CreatedAt || '';

        const payable = num(r.payableAmount ?? r.PayableAmount);
        const total = num(r.totalAmount ?? r.TotalAmount);
        const kdv = Math.max(0, total - payable);

        const isAccount = (r.isAccount === true || r.isAccount === 'YES') ? 'Evet'
            : (r.isAccount === false || r.isAccount === 'NO') ? 'Hayır'
                : '-';

        const status =
            r.status ||
            r.Status ||
            r.envelopeStatus ||
            (r.envelope && r.envelope.status) ||
            '';

        const responseCode =
            r.applicationResponseCode ||
            r.ApplicationResponseCode ||
            r.responseCode ||
            '';

        return {
            uuid,
            invoiceNumber,
            senderTitle,
            senderVkn,
            tipText,
            issueDate,
            createdDate,
            kdv,
            payable,
            isAccount,
            status,
            responseCode,
            raw: r
        };
    }

    function buildSearchParamsFromUI() {
        const p = {};

        // Senin eski script’te kullandığın parametreler (uyumlu)
        p.userId = State.userId;

        // Tarih aralığı
        const start = normalizeHtmlDateTime($('#IssueDateStart').val(), false);
        const end = normalizeHtmlDateTime($('#IssueDateStartEnd').val(), true);
        if (start) p.start = start;
        if (end) p.end = end;

        // Filtreler
        const IsArchive = $('#IsArchive').val();
        if (IsArchive !== '') p.isArchive = IsArchive;

        const ApplicationResponseCode = $('#ApplicationResponseCode').val();
        if (ApplicationResponseCode) p.applicationResponseCode = ApplicationResponseCode;

        const DocumentId = $('#DocumentId').val();
        if (DocumentId) p.documentId = DocumentId;

        const UUID = $('#UUID').val();
        if (UUID) p.uuid = UUID;

        const TargetIdentifier = $('#TargetIdentifier').val();
        if (TargetIdentifier) p.targetIdentifier = TargetIdentifier;

        const TargetTitle = $('#TargetTitle').val();
        if (TargetTitle) p.targetTitle = TargetTitle;

        // Detaylı alanlar (boşsa göndermiyoruz)
        const ProfileId = $('#ProfileId').val();
        if (ProfileId) p.profileId = ProfileId;

        const InvoiceTypeCode = $('#InvoiceTypeCode').val();
        if (InvoiceTypeCode) p.invoiceTypeCode = InvoiceTypeCode;

        const CurrencyCode = $('#CurrencyCode').val();
        if (CurrencyCode) p.currencyCode = CurrencyCode;

        const PayableAmount = $('#PayableAmount').val();
        if (PayableAmount) p.payableAmount = PayableAmount;

        const IntegrationYear = $('#IntegrationYear').val();
        const IntegrationMonth = $('#IntegrationMonth').val();
        if (IntegrationYear) p.integrationYear = IntegrationYear;
        if (IntegrationMonth && IntegrationMonth !== '-1') p.integrationMonth = IntegrationMonth;

        // sayfalama
        p.pageIndex = 1;
        p.pageSize = 100;
        p.isNew = true;

        return p;
    }

    function selectedUuids() {
        const ids = [];
        $('#myDataTable .rowchk:checked').each(function () {
            ids.push($(this).data('uuid'));
        });
        return ids;
    }

    function setTotals(items) {
        let kdvToplam = 0;
        let payableToplam = 0;
        for (const x of items) {
            kdvToplam += num(x.kdv);
            payableToplam += num(x.payable);
        }
        $('#totalKDVTaxableAmount').text('0,00 TL');
        $('#totalKDVAmount').text(fmtMoneyTR(kdvToplam) + ' TL');
        $('#totalPayableAmount').text(fmtMoneyTR(payableToplam) + ' TL');
    }

    function actionButtonsHtml(row) {
        const u = row.uuid;
        const invNo = row.invoiceNumber || '';
        return `
      <div class="btn-group btn-group-xs">
        <button class="btn btn-info" title="PDF" onclick="window.inboxInvoiceOpenPdf('${u}')">
          <i class="fa fa-file-pdf-o"></i>
        </button>
        <button class="btn btn-default" title="UBL(XML)" onclick="window.inboxInvoiceOpenUbl('${u}')">
          <i class="fa fa-file-code-o"></i>
        </button>
        <button class="btn btn-primary" title="Önizleme" onclick="window.inboxInvoicePreview('${u}')">
          <i class="fa fa-eye"></i>
        </button>
        <button class="btn btn-success" title="Kabul/Red" onclick="window.openCevaplaModal('${u}','${invNo.replace(/'/g, '')}')">
          <i class="fa fa-reply"></i>
        </button>
        <button class="btn btn-warning" title="Durum Sorgula" onclick="window.inboxInvoiceStatus('${u}')">
          <i class="fa fa-refresh"></i>
        </button>
      </div>
    `;
    }

    function renderTable(items) {
        setTotals(items);

        // DataTables varsa kullan, yoksa düz bas
        const hasDT = !!($.fn && $.fn.DataTable);

        if (hasDT) {
            if (State.dt) {
                State.dt.clear();
                State.dt.rows.add(items);
                State.dt.draw();
                return;
            }

            State.dt = $('#myDataTable').DataTable({
                data: items,
                destroy: true,
                paging: true,
                searching: true,
                order: [[5, 'desc']],
                columns: [
                    {
                        data: 'uuid',
                        orderable: false,
                        render: (data) => `<input type="checkbox" class="rowchk" data-uuid="${data}">`
                    },
                    {
                        data: null,
                        render: (r) => {
                            const no = r.invoiceNumber || '';
                            const ettn = r.uuid || '';
                            return `${no}${ettn ? `<br><small>${ettn}</small>` : ''}`;
                        }
                    },
                    { data: 'senderTitle' },
                    { data: 'senderVkn' },
                    { data: 'tipText' },
                    {
                        data: 'issueDate',
                        render: (d) => (d ? String(d).substring(0, 10) : '')
                    },
                    {
                        data: 'createdDate',
                        render: (d) => (d ? String(d).replace('T', ' ').substring(0, 16) : '')
                    },
                    {
                        data: 'kdv',
                        className: 'text-right',
                        render: (d) => fmtMoneyTR(d)
                    },
                    {
                        data: 'payable',
                        className: 'text-right',
                        render: (d) => fmtMoneyTR(d)
                    },
                    { data: 'isAccount', className: 'text-center' },
                    {
                        data: null,
                        className: 'text-center',
                        render: (r) => (r.status || r.responseCode || '')
                    },
                    {
                        data: null,
                        orderable: false,
                        className: 'text-center',
                        render: (r) => actionButtonsHtml(r)
                    }
                ]
            });

            // chkAll
            $('#chkAll').off('change').on('change', function () {
                const c = $(this).is(':checked');
                $('#myDataTable .rowchk').prop('checked', c);
            });

            return;
        }

        // DataTables yoksa:
        const $tbody = $('#myDataTable tbody');
        $tbody.empty();
        for (const row of items) {
            $tbody.append(`
        <tr>
          <td><input type="checkbox" class="rowchk" data-uuid="${row.uuid}"></td>
          <td>${row.invoiceNumber || ''}${row.uuid ? `<br><small>${row.uuid}</small>` : ''}</td>
          <td>${row.senderTitle || ''}</td>
          <td>${row.senderVkn || ''}</td>
          <td>${row.tipText || ''}</td>
          <td>${row.issueDate ? String(row.issueDate).substring(0, 10) : ''}</td>
          <td>${row.createdDate ? String(row.createdDate).replace('T', ' ').substring(0, 16) : ''}</td>
          <td class="text-right">${fmtMoneyTR(row.kdv)}</td>
          <td class="text-right">${fmtMoneyTR(row.payable)}</td>
          <td class="text-center">${row.isAccount}</td>
          <td class="text-center">${row.status || row.responseCode || ''}</td>
          <td class="text-center">${actionButtonsHtml(row)}</td>
        </tr>
      `);
        }

        $('#chkAll').off('change').on('change', function () {
            const c = $(this).is(':checked');
            $('#myDataTable .rowchk').prop('checked', c);
        });
    }

    async function loadList() {
        try {
            const params = buildSearchParamsFromUI();
            const res = await EinvoiceInboxApi.list(params);
            const arr = Array.isArray(res) ? res : (res?.items ? res.items : []);
            State.items = (arr || []).map(mapRow).filter(x => x.uuid);
            renderTable(State.items);
        } catch (e) {
            console.error('[InboxInvoice] list error:', e);
            toastErr(e?.message || 'Gelen faturalar alınamadı.');
            renderTable([]);
        }
    }

    // ===== PDF / UBL / Preview =====
    function openPdf(uuid, inModal) {
        const url = EinvoiceInboxApi.pdfUrl(uuid, { userId: State.userId, standardXslt: true });
        if (inModal) {
            $('#invoicePreviewFrame').attr('src', url);
            $('#invoicePreviewModal').modal('show');
        } else {
            global.open(url, '_blank');
        }
    }

    function openUbl(uuid) {
        const url = EinvoiceInboxApi.ublUrl(uuid, { userId: State.userId });
        global.open(url, '_blank');
    }

    // ===== Kabul / Red =====
    function openResponseModal(uuid, invoiceNo) {
        $('#respUuid').val(uuid);
        $('#respInvoiceNo').text(invoiceNo || '');
        $('#ddlResponseCode').val('KABUL');
        $('#txtResponseNote').val('');
        $('#responseModal').modal('show');
    }

    // ✅ GÜNCELLENDİ: API'ye invoiceId/status(int)/reason gönder
    async function sendApplicationResponse() {
        const uuid = $('#respUuid').val();
        const code = $('#ddlResponseCode').val();
        const note = $('#txtResponseNote').val();

        if (!uuid) return toastErr('UUID bulunamadı.');
        if (!code) return toastErr('Cevap tipi seçiniz.');

        const statusInt = mapUiResponseCodeToStatusInt(code);
        if (statusInt === null || Number.isNaN(statusInt)) {
            return toastErr('Cevap tipi (status) geçersiz. (KABUL/RED veya 0/1 bekleniyor)');
        }

        // İsteğe bağlı: Red ise sebep zorunlu (API validation bunu istiyor olabilir)
        if (statusInt !== 0 && !(note || '').trim()) {
            return toastErr('Red işlemi için açıklama (sebep) giriniz.');
        }

        // Curl ile birebir uyumlu body
        const body = {
            invoiceId: String(uuid),
            status: statusInt,                // ✅ integer
            reason: (note || '').trim()
        };

        try {
            // Yapıyı bozmadan: aynı imza, sadece query/body düzeltildi
            await EinvoiceInboxApi.sendApplicationResponse(
                uuid,
                { userId: State.userId },       // ✅ sadece userId query
                body                             // ✅ doğru body
            );

            toastOk(`Uygulama yanıtı gönderildi. (status=${statusInt})`);
            $('#responseModal').modal('hide');
            await loadList();
        } catch (e) {
            console.error('[InboxInvoice] send response error:', e);
            toastErr(extractApiErrorMessage(e));
        }
    }

    // ===== Durum sorgu =====
    async function status(uuid) {
        try {
            const res = await EinvoiceInboxApi.status(uuid, { userId: State.userId });
            toastOk(`Durum: ${typeof res === 'string' ? res : (res?.status || JSON.stringify(res))}`);
            await loadList();
        } catch (e) {
            // Endpoint yoksa dert değil, list refresh ile idare eder
            console.warn('[InboxInvoice] status endpoint çalışmadı, liste yenileniyor...', e);
            await loadList();
        }
    }

    // ===== Toplu İşlemler =====
    async function bulkArchive(val) {
        const uuids = selectedUuids();
        if (!uuids.length) return toastErr('Lütfen en az bir kayıt seçin.');

        try {
            await EinvoiceInboxApi.bulkArchive(
                { userId: State.userId },
                { uuids, isArchive: !!val }
            );
            toastOk(val ? 'Arşive alındı.' : 'Arşivden çıkarıldı.');
            await loadList();
        } catch (e) {
            console.error(e);
            toastErr(e?.message || 'Toplu arşiv işlemi başarısız. (Endpoint PATH.INBOX_BULK_ARCHIVE kontrol edin)');
        }
    }

    async function bulkRead(val) {
        const uuids = selectedUuids();
        if (!uuids.length) return toastErr('Lütfen en az bir kayıt seçin.');

        try {
            await EinvoiceInboxApi.bulkRead(
                { userId: State.userId },
                { uuids, isRead: !!val }
            );
            toastOk(val ? 'Okundu işaretlendi.' : 'Okunmadı işaretlendi.');
            await loadList();
        } catch (e) {
            console.error(e);
            toastErr(e?.message || 'Toplu okundu işlemi başarısız. (Endpoint PATH.INBOX_BULK_READ kontrol edin)');
        }
    }

    async function bulkPaid(val) {
        const uuids = selectedUuids();
        if (!uuids.length) return toastErr('Lütfen en az bir kayıt seçin.');

        try {
            await EinvoiceInboxApi.bulkPaid(
                { userId: State.userId },
                { uuids, isPaid: !!val }
            );
            toastOk(val ? 'Ödendi işaretlendi.' : 'Ödenmedi işaretlendi.');
            await loadList();
        } catch (e) {
            console.error(e);
            toastErr(e?.message || 'Toplu ödendi işlemi başarısız. (Endpoint PATH.INBOX_BULK_PAID kontrol edin)');
        }
    }

    // ===== Download all (PDF/XML/...) =====
    function downloadAll(type) {
        const uuids = selectedUuids();
        if (!uuids.length) return toastErr('Lütfen en az bir kayıt seçin.');

        // Şimdilik: her UUID için ayrı açıyoruz.
        // API’de zip/toplu endpoint’in varsa burada tek endpoint’e bağlarız.
        for (const u of uuids) {
            if (type === 'PDF') openPdf(u, false);
            else if (type === 'XML') openUbl(u);
            else openPdf(u, false);
        }
    }

    // ===== Clear / Search / VKN checkbox =====
    function vknAyracChk() {
        const multi = $('#TargetIdentifierChk').is(':checked');
        const $inp = $('#TargetIdentifier');
        $inp.val('');
        if (multi) $inp.attr('maxlength', '400').attr('placeholder', '111...,222...,333...');
        else $inp.attr('maxlength', '11').attr('placeholder', '11 hane');
    }

    async function clearSearchInputs() {
        $('#IsArchive,#ApplicationResponseCode,#ProfileId,#InvoiceTypeCode,#CurrencyCode,#IsCancelled,#IsPrinted,#Status,#IsAccount,#IsPaid,#PostaKutusuEtiketi').val('');
        $('#DocumentId,#UUID,#TargetIdentifier,#TargetTitle').val('');
        $('#TargetIdentifierChk').prop('checked', false);
        vknAyracChk();

        // tarih reset
        const now = new Date();
        const y = now.getFullYear();
        const m = String(now.getMonth() + 1).padStart(2, '0');
        const d = String(now.getDate()).padStart(2, '0');
        $('#IssueDateStart').val(`${y}-${m}-01T00:00`);
        $('#IssueDateStartEnd').val(`${y}-${m}-${d}T23:59`);

        $('#CreatedDateStart,#CreatedDateEnd').val('');
        $('#IntegrationMonth').val('-1');

        await loadList();
    }

    // ===== INIT =====
    function init() {
        if (!$) {
            console.error('[InboxInvoice] jQuery bulunamadı.');
            return;
        }

        try {
            State.userId = getCurrentUserIdForGib();
        } catch (e) {
            console.error(e);
            toastErr(e.message);
            return;
        }

        // Default tarih: ayın 1’i -> bugün
        (function initDefaultIssueDates() {
            const now = new Date();
            const y = now.getFullYear();
            const m = String(now.getMonth() + 1).padStart(2, '0');
            const d = String(now.getDate()).padStart(2, '0');
            if (!$('#IssueDateStart').val()) $('#IssueDateStart').val(`${y}-${m}-01T00:00`);
            if (!$('#IssueDateStartEnd').val()) $('#IssueDateStartEnd').val(`${y}-${m}-${d}T23:59`);
        })();

        // Year/Month küçük davranış
        function yearChange() {
            const now = new Date();
            const selY = Number($('#IntegrationYear').val());
            if (selY === now.getFullYear()) $('#IntegrationMonth').val(String(now.getMonth() + 1));
            else $('#IntegrationMonth').val('-1');
        }
        $('#IntegrationYear').on('change', yearChange);
        yearChange();

        // Detaylı arama ikon
        $('#btnDetayliArama').on('click', function () {
            const $i = $(this).find('.myCollapseIcon');
            setTimeout(() => { $i.toggleClass('fa-plus fa-minus'); }, 150);
        });

        // Enter ile arama
        $('#btnAramaYapEnter').on('keypress', 'input', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadList();
            }
        });

        // Response modal gönder
        $('#btnSendResponse').on('click', function (e) {
            e.preventDefault();
            sendApplicationResponse();
        });

        // Global (view onclick’leri için)
        global.gridSearch = () => loadList();
        global.clearSearchInputs = () => clearSearchInputs();
        global.vknAyracChk = () => vknAyracChk();

        global.invoiceTopluArsivIslem = (val) => bulkArchive(val);
        global.invoiceTopluOkunduIslem = (val) => bulkRead(val);
        global.invoiceTopluOdendiIslem = (val) => bulkPaid(val);

        global.getDownloadFileAll = (type) => downloadAll(type);

        // Bunlar menüde var; şimdilik “boş” ama patlamasın:
        global.invoiceSendMailModalCol = () => toastErr('Mail gönderme için API endpoint bağlanacak.');
        global.invoiceExcelAktarModal = () => toastErr('Excel aktarım için API endpoint bağlanacak.');
        global.sendLucaToplu = () => toastErr('Luca aktarım için API endpoint bağlanacak.');
        global.muhasebeyeTopluAktar = () => toastErr('ERP fiş aktarım için API endpoint bağlanacak.');
        global.getInvoiceTopluPrint = () => toastErr('Toplu yazdır için API endpoint bağlanacak.');

        // Row aksiyonları
        global.inboxInvoiceOpenPdf = (uuid) => openPdf(uuid, false);
        global.inboxInvoiceOpenUbl = (uuid) => openUbl(uuid);
        global.inboxInvoicePreview = (uuid) => openPdf(uuid, true);
        global.inboxInvoiceStatus = (uuid) => status(uuid);

        global.openCevaplaModal = (uuid, invNo) => openResponseModal(uuid, invNo);
        global.sendApplicationResponse = () => sendApplicationResponse();

        // ilk liste
        loadList();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})(window);
