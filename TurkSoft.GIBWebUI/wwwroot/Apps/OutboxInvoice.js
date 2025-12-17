// wwwroot/apps/einvoice/OutboxInvoice.js
import { EinvoiceOutboxApi } from '../Base/turkcellEfaturaApi.js';

(function (global) {
    'use strict';

    const $ = global.jQuery;

    function ok(m) { if (global.toastr?.success) toastr.success(m); else alert(m); }
    function err(m) { if (global.toastr?.error) toastr.error(m); else alert(m); }

    function num(v) {
        const n = Number(v);
        return Number.isFinite(n) ? n : 0;
    }
    function fmt(n) {
        return (Number(n) || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // dd.mm.yyyy -> yyyy-MM-dd
    function toIsoDateTR(str) {
        if (!str) return '';
        const p = String(str).split('.');
        if (p.length !== 3) return '';
        const dd = p[0].padStart(2, '0');
        const mm = p[1].padStart(2, '0');
        const yy = p[2]; // hem 2025 hem 25 gelirse çalışır ama 25 ise iso anlamsız olur; datepicker'ı 4 hane ayarlıyoruz
        return `${yy}-${mm}-${dd}`;
    }

    // yyyy-MM-dd -> yyyy-MM-dd HH:mm:ss
    function toDateTimeRange(dateIso, isEnd) {
        if (!dateIso) return '';
        return `${dateIso} ${isEnd ? '23:59:59' : '00:00:00'}`;
    }

    function getCurrentUserIdForGib() {
        let userId = 0;
        const hdn = document.getElementById('hdnUserId');
        if (hdn?.value) {
            const p = parseInt(hdn.value, 10);
            if (!isNaN(p) && p > 0) userId = p;
        }
        if (!userId && typeof global.currentUserId === 'number' && global.currentUserId > 0) userId = global.currentUserId;

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

    const State = { userId: 0, items: [], dt: null };

    function mapRow(raw) {
        const r = raw || {};
        const uuid = r.uuid || r.UUID || r.envelopeId || r.id || '';
        const invoiceNumber = r.invoiceNumber || r.documentId || r.DocumentId || '';
        const targetTitle = r.targetTitle || r.receiverTitle || r.TargetTitle || '';
        const targetId = r.targetIdentifier || r.receiverIdentifier || r.TargetIdentifier || '';
        const profileId = r.profileId || r.ProfileId || '';
        const invType = r.invoiceTypeCode || r.InvoiceTypeCode || r.type || '';
        const tipText = (invType && profileId) ? `${invType} / ${profileId}` : (invType || profileId || '');

        const issueDate = r.issueDate || r.IssueDate || r.executionDate || '';
        const createdDate = r.createdDate || r.CreatedDate || '';

        const payable = num(r.payableAmount ?? r.PayableAmount);
        const total = num(r.totalAmount ?? r.TotalAmount);
        const kdv = num(r.kdvAmount ?? r.KdvAmount) || Math.max(0, total - payable);

        const isAccount =
            (r.isAccount === true || r.isAccount === 'YES') ? 'Evet' :
                (r.isAccount === false || r.isAccount === 'NO') ? 'Hayır' : '-';

        const status = r.status || r.Status || r.envelopeStatus || '';
        const resp = r.applicationResponseCode || r.ApplicationResponseCode || '';

        return { uuid, invoiceNumber, targetTitle, targetId, tipText, issueDate, createdDate, kdv, payable, isAccount, status, resp, raw: r };
    }

    function buildParamsFromUI() {
        const p = {};
        p.userId = State.userId;

        // Tarih aralığı (datepicker dd.mm.yyyy)
        const sIso = toIsoDateTR($('#IssueDateStart').val());
        const eIso = toIsoDateTR($('#IssueDateStartEnd').val());
        const start = toDateTimeRange(sIso, false);
        const end = toDateTimeRange(eIso, true);
        if (start) p.start = start;
        if (end) p.end = end;

        // İşlem tarihi (opsiyonel)
        const csIso = toIsoDateTR($('#CreatedDateStart').val());
        const ceIso = toIsoDateTR($('#CreatedDateEnd').val());
        const cStart = toDateTimeRange(csIso, false);
        const cEnd = toDateTimeRange(ceIso, true);
        if (cStart) p.createdStart = cStart;
        if (cEnd) p.createdEnd = cEnd;

        // Filtreler
        const IsArchive = $('#IsArchive').val();
        if (IsArchive !== '') p.isArchive = IsArchive;

        const ApplicationResponseCode = $('#ApplicationResponseCode').val();
        if (ApplicationResponseCode) p.applicationResponseCode = ApplicationResponseCode;

        const DocumentId = $('#DocumentId').val();
        if (DocumentId) p.documentId = DocumentId;

        const UUID = $('#UUID').val();
        if (UUID) p.uuid = UUID;

        const TargetTitle = $('#TargetTitle').val();
        if (TargetTitle) p.targetTitle = TargetTitle;

        const ProfileId = $('#ProfileId').val();
        if (ProfileId) p.profileId = ProfileId;

        const InvoiceTypeCode = $('#InvoiceTypeCode').val();
        if (InvoiceTypeCode) p.invoiceTypeCode = InvoiceTypeCode;

        const IsPaid = $('#IsPaid').val();
        if (IsPaid) p.isPaid = IsPaid;

        const CurrencyCode = $('#CurrencyCode').val();
        if (CurrencyCode) p.currencyCode = CurrencyCode;

        const Status = $('#Status').val();
        if (Status) p.status = Status;

        const PayableAmount = $('#PayableAmount').val();
        if (PayableAmount) p.payableAmountMin = PayableAmount;

        const IsCancelled = $('#IsCancelled').val();
        if (IsCancelled) p.isCancelled = IsCancelled;

        const SourceAlias = $('#SourceAlias').val();
        if (SourceAlias) p.sourceAlias = SourceAlias;

        const IsPrinted = $('#IsPrinted').val();
        if (IsPrinted) p.isPrinted = IsPrinted;

        const IsAccount = $('#IsAccount').val();
        if (IsAccount) p.isAccount = IsAccount;

        const year = $('#IntegrationYear').val();
        const month = $('#IntegrationMonth').val();
        if (year) p.integrationYear = year;
        if (month && month !== '-1') p.integrationMonth = month;

        // Alıcı VKN/TCKN (çoklu)
        const targetIdentifier = $('#TargetIdentifier').val();
        if (targetIdentifier) {
            if ($('#TargetIdentifierChk').is(':checked')) {
                p.targetIdentifier = targetIdentifier.split(',').map(s => s.trim()).filter(Boolean);
            } else {
                p.targetIdentifier = targetIdentifier.trim();
            }
        }

        const branches = $('#subeler').val();
        if (branches && branches.length) p.branchIds = branches;

        // sayfalama (client-side tablo basacağız)
        p.pageIndex = 1;
        p.pageSize = 200;

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
        let kdvTop = 0, payTop = 0;
        for (const x of items) {
            kdvTop += num(x.kdv);
            payTop += num(x.payable);
        }
        $('#totalKDVTaxableAmount').text('0,00 TL');
        $('#totalKDVAmount').text(fmt(kdvTop) + ' TL');
        $('#totalPayableAmount').text(fmt(payTop) + ' TL');
    }

    function actionsHtml(r) {
        const u = r.uuid;
        const doc = (r.invoiceNumber || '').replace(/'/g, '');
        return `
      <div class="btn-group btn-group-xs">
        <button class="btn btn-info" title="Önizle (PDF)" onclick="window.invoicePreview('${u}')">
          <i class="fa fa-eye"></i>
        </button>
        <button class="btn btn-default" title="UBL(XML)" onclick="window.outboxOpenUbl('${u}')">
          <i class="fa fa-file-code-o"></i>
        </button>
        <button class="btn btn-primary" title="Mail" onclick="window.invoiceSendMailModal('${u}')">
          <i class="fa fa-envelope"></i>
        </button>
        <button class="btn btn-danger" title="GİB İptal/İtiraz" onclick="window.openGibCancel('${doc}','${u}')">
          <i class="fa fa-times"></i>
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
                        data: 'uuid',
                        orderable: false,
                        render: (d) => `<input type="checkbox" class="rowchk" data-uuid="${d}">`
                    },
                    {
                        data: null,
                        render: (r) => `${r.invoiceNumber || ''}${r.uuid ? `<br><small>${r.uuid}</small>` : ''}`
                    },
                    { data: 'targetTitle' },
                    { data: 'targetId' },
                    { data: 'tipText' },
                    { data: 'issueDate', render: (d) => (d ? String(d).substring(0, 10) : '') },
                    { data: 'createdDate', render: (d) => (d ? String(d).replace('T', ' ').substring(0, 16) : '') },
                    { data: 'kdv', className: 'text-right', render: (d) => fmt(d) },
                    { data: 'payable', className: 'text-right', render: (d) => fmt(d) },
                    { data: 'isAccount', className: 'text-center' },
                    {
                        data: null, className: 'text-center',
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

        // DataTables yoksa düz bas
        const $tb = $('#myDataTable tbody');
        $tb.empty();
        for (const r of items) {
            $tb.append(`
        <tr>
          <td><input type="checkbox" class="rowchk" data-uuid="${r.uuid}"></td>
          <td>${r.invoiceNumber || ''}${r.uuid ? `<br><small>${r.uuid}</small>` : ''}</td>
          <td>${r.targetTitle || ''}</td>
          <td>${r.targetId || ''}</td>
          <td>${r.tipText || ''}</td>
          <td>${r.issueDate ? String(r.issueDate).substring(0, 10) : ''}</td>
          <td>${r.createdDate ? String(r.createdDate).replace('T', ' ').substring(0, 16) : ''}</td>
          <td class="text-right">${fmt(r.kdv)}</td>
          <td class="text-right">${fmt(r.payable)}</td>
          <td class="text-center">${r.isAccount}</td>
          <td class="text-center">${(r.status || '') + (r.resp ? ` / ${r.resp}` : '')}</td>
          <td class="text-center">${actionsHtml(r)}</td>
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
            const params = buildParamsFromUI();
            const res = await EinvoiceOutboxApi.list(params);

            const arr = Array.isArray(res) ? res : (res?.items ? res.items : []);
            State.items = (arr || []).map(mapRow).filter(x => x.uuid);

            renderTable(State.items);
        } catch (e) {
            console.error('[OutboxInvoice] list error:', e);
            err(e?.message || 'Giden faturalar alınamadı.');
            renderTable([]);
        }
    }

    // ===== PDF / UBL =====
    function openPdf(uuid, inModal) {
        const url = EinvoiceOutboxApi.pdfUrl(uuid, { userId: State.userId, standardXslt: true });

        // Önizleme modal’ı varsa kullan, yoksa yeni sekme
        const $frame = $('#invoicePreviewFrame');
        const $modal = $('#invoicePreviewModal');
        if (inModal && $frame.length && $modal.length) {
            $frame.attr('src', url);
            $modal.modal('show');
        } else {
            global.open(url, '_blank');
        }
    }

    function openUbl(uuid) {
        const url = EinvoiceOutboxApi.ublUrl(uuid, { userId: State.userId });
        global.open(url, '_blank');
    }

    // ===== Toplu işlemler =====
    async function bulkArchive(flag) {
        const uuids = selectedUuids();
        if (!uuids.length) return err('Lütfen en az bir kayıt seçin.');

        try {
            await EinvoiceOutboxApi.bulkArchive({ userId: State.userId }, { uuids, isArchive: !!flag });
            ok(flag ? 'Arşive alındı.' : 'Arşivden çıkarıldı.');
            await loadList();
        } catch (e) {
            console.error(e);
            err(e?.message || 'Toplu arşiv işlemi başarısız. (PATH.OUTBOX_BULK_ARCHIVE kontrol edin)');
        }
    }

    async function bulkPaid(flag) {
        const uuids = selectedUuids();
        if (!uuids.length) return err('Lütfen en az bir kayıt seçin.');

        try {
            await EinvoiceOutboxApi.bulkPaid({ userId: State.userId }, { uuids, isPaid: !!flag });
            ok(flag ? 'Ödendi işaretlendi.' : 'Ödenmedi işaretlendi.');
            await loadList();
        } catch (e) {
            console.error(e);
            err(e?.message || 'Toplu ödendi işlemi başarısız. (PATH.OUTBOX_BULK_PAID kontrol edin)');
        }
    }

    function downloadAll(type) {
        const uuids = selectedUuids();
        if (!uuids.length) return err('Lütfen en az bir kayıt seçin.');

        // Zip/toplu endpoint’in yoksa basit fallback: her birini ayrı aç
        for (const u of uuids) {
            if (type === 'PDF') openPdf(u, false);
            else if (type === 'XML') openUbl(u);
            else openPdf(u, false);
        }
    }

    // ===== Mail =====
    async function sendMail(dto) {
        // dto: { uuids, receiverMail, title, receiverName, attachXml, attachPdf }
        try {
            await EinvoiceOutboxApi.sendMail(
                { userId: State.userId },
                dto
            );
            ok('E-posta gönderildi.');
        } catch (e) {
            console.error(e);
            err(e?.message || 'E-posta gönderilemedi. (PATH.OUTBOX_SEND_MAIL kontrol edin)');
        }
    }

    // ===== GİB iptal/itiraz flag =====
    async function gibCancelFlag(uuid, flag) {
        try {
            await EinvoiceOutboxApi.gibCancelFlag({ userId: State.userId }, { uuid, value: !!flag });
            ok('GİB iptal/itiraz işareti güncellendi.');
            await loadList();
        } catch (e) {
            console.error(e);
            err(e?.message || 'GİB iptal/itiraz güncellenemedi. (PATH.OUTBOX_GIB_CANCEL_FLAG kontrol edin)');
        }
    }

    // ===== VKN çoklu toggle (view onclick kullanıyor) =====
    function vknAyracChk() {
        const multi = $('#TargetIdentifierChk').is(':checked');
        const $inp = $('#TargetIdentifier');
        $inp.val('');
        if (multi) $inp.attr('maxlength', '400').attr('placeholder', '111...,222...,333...');
        else $inp.attr('maxlength', '11').attr('placeholder', '11 hane');
    }

    function clearSearchInputs() {
        $('#IsArchive,#ApplicationResponseCode,#Status,#ProfileId,#InvoiceTypeCode,#IsPaid,#CurrencyCode,#IsCancelled,#SourceAlias,#IsPrinted,#IsAccount').val('');
        $('#IssueDateStart,#IssueDateStartEnd,#CreatedDateStart,#CreatedDateEnd').val('');
        $('#DocumentId,#UUID,#TargetIdentifier,#TargetTitle').val('');
        $('#TargetIdentifierChk').prop('checked', false);
        vknAyracChk();

        $('#IntegrationMonth').val('-1');
        yearChange();

        if ($.fn.select2) {
            $('#CurrencyCode').trigger('change');
            $('#subeler').val(null).trigger('change');
        }

        loadList();
    }

    function yearChange() {
        const now = new Date();
        const selY = Number($('#IntegrationYear').val());
        if (selY === now.getFullYear()) $('#IntegrationMonth').val(String(now.getMonth() + 1));
        else $('#IntegrationMonth').val('-1');
    }

    // ✅ FIX: jQuery UI Datepicker + Bootstrap Datepicker uyumlu init
    function initDatepicker() {
        if (!$ || !$.fn) return;

        const months = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
        const monthsShort = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
        const days = ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"];
        const daysShort = ["Paz", "Pts", "Sal", "Çar", "Per", "Cum", "Cts"];
        const daysMin = ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"];

        // 1) jQuery UI Datepicker varsa
        if ($.datepicker && typeof $.datepicker.setDefaults === "function") {
            const TR_UI = {
                closeText: "Kapat", prevText: "Önceki", nextText: "Sonraki", currentText: "Bugün",
                monthNames: months, monthNamesShort: monthsShort,
                dayNames: days, dayNamesShort: daysShort, dayNamesMin: daysMin,
                dateFormat: "dd.mm.yy", firstDay: 1, isRTL: false
            };

            $.datepicker.setDefaults(TR_UI);
            $('.datepicker').datepicker({
                changeMonth: true,
                changeYear: true,
                showAnim: "fadeIn",
                dateFormat: "dd.mm.yy"
            });
            return;
        }

        // 2) bootstrap-datepicker varsa ($.fn.datepicker var ama $.datepicker yok)
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

    function init() {
        if (!$) {
            console.error('[OutboxInvoice] jQuery yok.');
            return;
        }

        try {
            State.userId = getCurrentUserIdForGib();
        } catch (e) {
            console.error(e);
            err(e.message);
            return;
        }

        initDatepicker();

        if ($.fn.select2) {
            $('#CurrencyCode,#subeler').select2({ width: '100%' });
        }

        // Detaylı Arama ikon
        $('#btnDetayliArama').on('click', function () {
            const $i = $(this).find('.myCollapseIcon');
            setTimeout(() => $i.toggleClass('fa-plus fa-minus'), 150);
        });

        $('#IntegrationYear').on('change', yearChange);
        yearChange();

        // View onclick
        global.vknAyracChk = () => vknAyracChk();

        // Search / Clear
        $('#btnSearch').on('click', loadList);
        $('#btnClear').on('click', clearSearchInputs);
        $('#btnAramaYapEnter').on('keypress', 'input', function (e) {
            if (e.which === 13) { e.preventDefault(); loadList(); }
        });

        // Bulk menü
        $('.dropdown-menu').on('click', 'a.bulk', function (e) {
            e.preventDefault();
            const action = String($(this).data('action') || '');

            if (action === 'archive-1') return bulkArchive(true);
            if (action === 'archive-0') return bulkArchive(false);
            if (action === 'paid-1') return bulkPaid(true);
            if (action === 'paid-0') return bulkPaid(false);

            if (action === 'envelope') return err('Zarf sorgulama bu entegrasyonda kullanılmıyor.');

            if (action === 'dl-XML') return downloadAll('XML');
            if (action === 'dl-PDF') return downloadAll('PDF');
            if (action === 'dl-TPDF') return downloadAll('TPDF');
            if (action === 'dl-HTML') return downloadAll('HTML');
            if (action === 'dl-JPG') return downloadAll('JPG');

            if (action === 'mail') return err('Toplu mail için modal/endpoint bağlanacak.');
            if (action === 'excel-selected' || action === 'excel-all') return err('Excel aktarım endpoint’i bağlanacak.');
            if (action === 'luca') return err('Luca aktarım endpoint’i bağlanacak.');
            if (action === 'erp') return err('ERP fiş aktarım endpoint’i bağlanacak.');
            if (action === 'print') return err('Toplu yazdır endpoint’i bağlanacak.');
        });

        // Row aksiyon fonksiyonları (render HTML çağırıyor)
        global.invoicePreview = (uuid) => openPdf(uuid, true);
        global.outboxOpenUbl = (uuid) => openUbl(uuid);

        global.invoiceSendMailModal = (uuid) => {
            const $m = $('#modal-mail');
            if ($m.length) {
                $m.data('uuid', uuid).modal('show');
                return;
            }

            const receiverMail = prompt('Alıcı e-posta:');
            if (!receiverMail) return;
            sendMail({
                uuids: [uuid],
                receiverMail,
                title: 'Fatura',
                receiverName: 'Alıcı',
                attachXml: true,
                attachPdf: true
            });
        };

        global.openGibCancel = (docNo, uuid) => {
            const $modal = $('#modal-cancel');
            if ($modal.length) {
                $('#modalGibIptal').text(docNo || '');
                $modal.data('uuid', uuid).modal('show');
                return;
            }

            const yes = confirm('GİB iptal/itiraz işaretini AKTİF etmek istiyor musun?');
            gibCancelFlag(uuid, yes);
        };

        // modal-mail send butonu varsa
        $(document).on('click', '#sendMailBtn', async function (e) {
            e.preventDefault();
            const $m = $('#modal-mail');
            if (!$m.length) return;

            const uuid = $m.data('uuid');
            const uuids = $m.data('uuids') || (uuid ? [uuid] : []);
            if (!uuids.length) return err('Seçim yok.');

            const dto = {
                uuids,
                receiverMail: $('#ReceiverMail').val(),
                title: $('#Title').val(),
                receiverName: $('#ReceiverFirstnameLastname').val(),
                attachXml: $('#sendXml').is(':checked'),
                attachPdf: $('#sendPdf').is(':checked'),
            };

            if (!dto.receiverMail || !dto.title || !dto.receiverName) return err('E-posta, konu ve alıcı adı zorunlu.');
            await sendMail(dto);
            $m.modal('hide');
        });

        // ilk liste
        loadList();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})(window);
