// wwwroot/apps/earchive/OutboxEarchiveInvoice.js
import { EarchiveOutboxApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    'use strict';

    // ===== Helpers =====
    const TR_DATE = {
        closeText: "Kapat", prevText: "Önceki", nextText: "Sonraki", currentText: "Bugün",
        monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
        monthNamesShort: ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"],
        dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
        dayNamesShort: ["Paz", "Pts", "Sal", "Çar", "Per", "Cum", "Cts"],
        dayNamesMin: ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"],
        dateFormat: "dd.mm.yy", firstDay: 1, isRTL: false
    };

    function toApiDate(str) { // dd.MM.yyyy -> yyyy-MM-dd
        if (!str) return "";
        const p = String(str).split(".");
        if (p.length !== 3) return "";
        return `${p[2]}-${String(p[1]).padStart(2, "0")}-${String(p[0]).padStart(2, "0")}`;
    }

    function num(v) { const n = +((v == null) ? 0 : v); return isNaN(n) ? 0 : n; }

    function fmtMoney(n, c = 2, d = ",", t = ".") {
        c = isNaN(c = Math.abs(c)) ? 2 : c;
        const s = n < 0 ? "-" : "";
        let i = String(parseInt(n = Math.abs(Number(n) || 0).toFixed(c)));
        const j = (j => (j = i.length) > 3 ? j % 3 : 0)();
        return s +
            (j ? i.substr(0, j) + t : "") +
            i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) +
            (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
    }

    function notifyOk(m) { window.toastr?.success ? toastr.success(m) : alert(m); }
    function notifyErr(m) { window.toastr?.error ? toastr.error(m) : alert(m); }

    // meta api-base -> window.open için
    function getApiBase() {
        const meta = document.querySelector('meta[name="api-base"]');
        if (meta?.content) return meta.content.replace(/\/+$/, '');
        if (window.__API_BASE) return String(window.__API_BASE).replace(/\/+$/, '');
        const body = document.getElementById('MainBody');
        if (body?.dataset?.apiBase) return String(body.dataset.apiBase).replace(/\/+$/, '');
        return '';
    }

    function qs(params) {
        const usp = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v == null || v === '') return;
            if (Array.isArray(v)) v.forEach(x => usp.append(k, x));
            else usp.append(k, String(v));
        });
        const s = usp.toString();
        return s ? `?${s}` : '';
    }

    function openApi(resourcePath, params) {
        const base = getApiBase();
        if (!base) return notifyErr('API base bulunamadı (meta api-base).');
        const url = `${base}/TurkcellEFatura/${resourcePath}${qs(params)}`;
        window.open(url, '_blank');
    }

    // ===== jQuery required =====
    if (!window.jQuery) {
        console.error('[OutboxEarchiveInvoice] jQuery yok.');
        return;
    }
    const $ = window.jQuery;

    // ===== UI Init: datepicker/select2/year/month =====
    // ✅ FIX: Hem jQuery UI datepicker hem bootstrap-datepicker ile uyumlu + patlamaz
    function initDatepicker($els) {
        if (!$els || !$els.length) return;

        if (!$.fn.datepicker) { // hiç datepicker yok
            $els.attr('placeholder', 'gg.aa.yyyy');
            return;
        }

        // Önce bootstrap-datepicker kontrolü (ikisinin aynı anda yüklü olduğu projelerde çakışmayı engeller)
        const isBs = !!($.fn.datepicker && $.fn.datepicker.dates);
        const isJqui = !!($.ui && $.ui.datepicker);

        if (isBs) {
            // TR lokalizasyonu yoksa ekle
            if (!$.fn.datepicker.dates.tr) {
                $.fn.datepicker.dates.tr = {
                    days: TR_DATE.dayNames,
                    daysShort: TR_DATE.dayNamesShort,
                    daysMin: TR_DATE.dayNamesMin,
                    months: TR_DATE.monthNames,
                    monthsShort: TR_DATE.monthNamesShort,
                    today: TR_DATE.currentText || "Bugün",
                    clear: TR_DATE.closeText || "Kapat",
                    format: "dd.mm.yyyy",
                    weekStart: TR_DATE.firstDay ?? 1
                };
            }

            $els.datepicker({
                language: 'tr',
                format: 'dd.mm.yyyy',
                autoclose: true,
                todayHighlight: true
            });
            return;
        }

        if (isJqui) {
            // jQuery UI setDefaults varsa uygula
            if ($.datepicker && typeof $.datepicker.setDefaults === 'function') {
                $.datepicker.setDefaults(TR_DATE);
            }
            $els.datepicker({ changeMonth: true, changeYear: true, showAnim: "fadeIn" });
            return;
        }

        // bilinmeyen datepicker -> en azından çalıştır
        try { $els.datepicker(); } catch (_) { /* ignore */ }
    }

    function linkRange($s, $e) {
        if (!$.fn.datepicker) return;

        const isBs = !!($.fn.datepicker && $.fn.datepicker.dates);
        const isJqui = !!($.ui && $.ui.datepicker);

        if (isBs) {
            // bootstrap-datepicker
            $s.on("changeDate change", () => {
                const v = $s.val();
                if (v) $e.datepicker("setStartDate", v);
            });
            $e.on("changeDate change", () => {
                const v = $e.val();
                if (v) $s.datepicker("setEndDate", v);
            });
            return;
        }

        if (isJqui) {
            // jQuery UI datepicker
            $s.on("change", () => {
                const d = $s.datepicker("getDate");
                if (d) $e.datepicker("option", "minDate", d);
            });
            $e.on("change", () => {
                const d = $e.datepicker("getDate");
                if (d) $s.datepicker("option", "maxDate", d);
            });
        }
    }

    function yearChange() {
        const now = new Date();
        const selY = +$('#IntegrationYear').val();
        if (selY === now.getFullYear()) $('#IntegrationMonth').val(String(now.getMonth() + 1));
        else $('#IntegrationMonth').val('-1');
    }

    // VKN çoklu
    window.vknAyracChk = function () {
        const multi = $('#TargetIdentifierChk').is(':checked');
        const $inp = $('#TargetIdentifier');
        $inp.val('');
        if (multi) $inp.attr('maxlength', '400').attr('placeholder', '111...,222...,333...');
        else $inp.attr('maxlength', '11').attr('placeholder', '11 hane');
    };

    // Detaylı arama ikon
    $('#btnDetayliArama').on('click', function () {
        const $i = $(this).find('.myCollapseIcon');
        setTimeout(() => $i.toggleClass('fa-plus fa-minus'), 150);
    });

    initDatepicker($('.datepicker'));
    linkRange($('#IssueDateStart'), $('#IssueDateStartEnd'));
    linkRange($('#CreatedDateStart'), $('#CreatedDateEnd'));

    if ($.fn.select2) {
        $('#CurrencyCode,#subeler').select2({ width: '100%' });
    }

    $('#IntegrationYear').on('change', yearChange);
    yearChange();

    $('#TargetIdentifierChk').on('change', window.vknAyracChk);

    // ===== Filters =====
    function buildFilters() {
        let ids = $('#TargetIdentifier').val() || "";
        if ($('#TargetIdentifierChk').is(':checked')) {
            ids = ids.split(',').map(s => $.trim(s)).filter(Boolean);
        }
        return {
            IsArchive: $('#IsArchive').val(),
            Status: $('#Status').val(),
            IssueDateStart: toApiDate($('#IssueDateStart').val()),
            IssueDateEnd: toApiDate($('#IssueDateStartEnd').val()),
            DocumentId: $('#DocumentId').val(),
            UUID: $('#UUID').val(),
            TargetIdentifier: ids,
            TargetTitle: $('#TargetTitle').val(),

            CreatedDateStart: toApiDate($('#CreatedDateStart').val()),
            CreatedDateEnd: toApiDate($('#CreatedDateEnd').val()),

            IsObjected: $('#IsObjected').val(),
            InvoiceTypeCode: $('#InvoiceTypeCode').val(),
            IsPaid: $('#IsPaid').val(),
            HasEMail: $('#HasEMail').val(),
            PayableAmountMin: $('#PayableAmount').val(),
            CurrencyCode: $('#CurrencyCode').val(),
            IsCanceled: $('#IsCanceled').val(),
            IsPrinted: $('#IsPrinted').val(),
            IsAccount: $('#IsAccount').val(),

            Year: $('#IntegrationYear').val(),
            Month: $('#IntegrationMonth').val(),
            BranchIds: $('#subeler').val() || []
        };
    }

    // ===== Footer totals =====
    let table = null;
    function updateFooters(tot) {
        if (tot) {
            $('#totalKDVTaxableAmount').text(fmtMoney(num(tot.totalKdvTaxable), 2, ",", ".") + " TL");
            $('#totalKDVAmount').text(fmtMoney(num(tot.totalKdv), 2, ",", ".") + " TL");
            $('#totalPayableAmount').text(fmtMoney(num(tot.totalPayable), 2, ",", ".") + " TL");
            return;
        }
        if (!table) return;
        let kdv = 0, pay = 0, matrah = 0;
        table.rows({ page: 'current' }).every(function () {
            const d = this.data() || {};
            kdv += num(d.KdvAmount ?? d.kdvAmount ?? d.KDV);
            pay += num(d.PayableAmount ?? d.payableAmount ?? d.OdenecekTutar);
            matrah += num(d.KdvTaxable ?? d.kdvTaxable ?? d.KDVMatrah);
        });
        $('#totalKDVTaxableAmount').text(fmtMoney(matrah, 2, ",", ".") + " TL");
        $('#totalKDVAmount').text(fmtMoney(kdv, 2, ",", ".") + " TL");
        $('#totalPayableAmount').text(fmtMoney(pay, 2, ",", ".") + " TL");
    }

    // ===== DataTables init =====
    function initTable() {
        if (!$.fn.DataTable) {
            notifyErr('DataTables yüklü değil.');
            return;
        }

        table = $('#myDataTable').DataTable({
            processing: true,
            serverSide: true,
            deferRender: true,
            order: [[6, 'desc']],
            pageLength: 25,
            lengthMenu: [[25, 50, 75, 100, 250, 500, 1000], [25, 50, 75, 100, 250, 500, "1.000"]],
            language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json' },

            ajax: async function (dt, callback) {
                try {
                    const res = await EarchiveOutboxApi.search({ dt, filters: buildFilters() });

                    // Backend zaten dt formatında döndürüyorsa direkt kullan
                    const draw = res?.draw ?? dt.draw;
                    const recordsTotal = res?.recordsTotal ?? res?.total ?? 0;
                    const recordsFiltered = res?.recordsFiltered ?? res?.filtered ?? recordsTotal;
                    const data = res?.data ?? res?.items ?? [];

                    if (res?.totals) updateFooters(res.totals);
                    else updateFooters(null);

                    callback({ draw, recordsTotal, recordsFiltered, data });
                } catch (ex) {
                    console.error('[OutboxEarchiveInvoice] search error:', ex);
                    notifyErr(ex?.message || 'Liste alınamadı.');
                    updateFooters(null);
                    callback({ draw: dt.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                }
            },

            columns: [
                {
                    data: null, orderable: false, searchable: false,
                    render: (d, t, r) => `<input type="checkbox" class="rowchk" data-id="${(r.UUID || r.uuid || '')}">`
                },
                {
                    data: null,
                    render: (d) => `${d.DocumentId || d.documentId || ''}${(d.UUID || d.uuid) ? `<br><small>${d.UUID || d.uuid}</small>` : ''}`
                },
                { data: 'TargetTitle', defaultContent: '' },
                { data: 'TargetIdentifier', defaultContent: '' },
                {
                    data: null,
                    render: (d) => `${d.InvoiceTypeCode || ''}${d.Scenario ? '/' + d.Scenario : ''}`
                },
                { data: 'IssueDate', render: (v) => v ? String(v).substr(0, 10) : '' },
                { data: 'CreatedDate', render: (v) => v ? String(v).replace('T', ' ').substr(0, 16) : '' },
                { data: 'KdvAmount', className: 'text-right', render: (v) => fmtMoney(num(v), 2, ",", ".") },
                { data: 'PayableAmount', className: 'text-right', render: (v) => fmtMoney(num(v), 2, ",", ".") },
                { data: 'IsAccount', className: 'text-center', render: (v) => (v === 'YES' || v === true) ? 'Evet' : 'Hayır' },
                { data: 'Status', className: 'text-center', defaultContent: '' },
                { data: 'HasEMail', className: 'text-center', render: (v) => (v === 'YES' || v === true) ? 'Gönderildi' : 'Gönderilmedi' },
                {
                    data: null, orderable: false, searchable: false, className: 'text-center',
                    render: (d) => {
                        const uid = d.UUID || '';
                        const doc = d.DocumentId || '';
                        return `
              <div class="btn-group btn-group-xs">
                <button class="btn btn-info" title="Önizle" onclick="invoicePreview('${uid}')"><i class="fa fa-eye"></i></button>
                <button class="btn btn-primary" title="Mail" onclick="invoiceSendMailModal('${uid}')"><i class="fa fa-envelope"></i></button>
                <button class="btn btn-warning" title="İtiraz" onclick="openItiraz('${doc}','${uid}')"><i class="fa fa-exclamation"></i></button>
                <button class="btn btn-danger" title="İptal" onclick="openIptal('${doc}','${uid}')"><i class="fa fa-times"></i></button>
              </div>`;
                    }
                }
            ],

            drawCallback: function () {
                $('#chkAll').prop('checked', false);
            }
        });

        $('#myDataTable').on('change', '#chkAll', function () {
            const c = $(this).is(':checked');
            $('#myDataTable .rowchk').prop('checked', c);
        });
    }

    initTable();

    // ===== Search/Clear/Enter =====
    function gridSearch() { table?.ajax?.reload?.(); }
    window.gridSearch = gridSearch;

    function clearSearchInputs() {
        $('#IsArchive,#Status,#IsObjected,#InvoiceTypeCode,#IsPaid,#HasEMail,#CurrencyCode,#IsCanceled,#IsPrinted,#IsAccount').val('');
        $('#IssueDateStart,#IssueDateStartEnd,#CreatedDateStart,#CreatedDateEnd').val('');
        $('#DocumentId,#UUID,#TargetIdentifier,#TargetTitle,#PayableAmount').val('');
        $('#TargetIdentifierChk').prop('checked', false);
        window.vknAyracChk();
        $('#IntegrationMonth').val('-1'); yearChange();

        if ($.fn.select2) {
            $('#CurrencyCode').trigger('change');
            $('#subeler').val(null).trigger('change');
        }
        gridSearch();
    }

    $('#btnSearch').on('click', gridSearch);
    $('#btnClear').on('click', clearSearchInputs);

    $('#btnAramaYapEnter').on('keypress', 'input', function (e) {
        if (e.which === 13) { e.preventDefault(); gridSearch(); }
    });

    // ===== Seçili UUID =====
    function getSelectedUUIDs() {
        const ids = [];
        $('#myDataTable .rowchk:checked').each(function () { ids.push($(this).data('id')); });
        return ids;
    }

    // ===== Bulk menu =====
    $('.dropdown-menu').on('click', 'a.bulk', async function (e) {
        e.preventDefault();
        const action = $(this).data('action');
        const uuids = getSelectedUUIDs();

        if (action !== 'excel-all' && !uuids.length) return notifyErr('Lütfen en az bir kayıt seçin.');

        try {
            switch (action) {
                case 'archive-1': await EarchiveOutboxApi.setArchive({ uuids, value: true }); notifyOk('Arşive alındı.'); return gridSearch();
                case 'archive-0': await EarchiveOutboxApi.setArchive({ uuids, value: false }); notifyOk('Arşivden çıkarıldı.'); return gridSearch();
                case 'paid-1': await EarchiveOutboxApi.setPaid({ uuids, value: true }); notifyOk('Ödendi işaretlendi.'); return gridSearch();
                case 'paid-0': await EarchiveOutboxApi.setPaid({ uuids, value: false }); notifyOk('Ödenmedi işaretlendi.'); return gridSearch();

                case 'mail': return window.invoiceSendMailModalCol?.(uuids, true);
                case 'excel-selected': return window.excelAktarModal?.(false, uuids);
                case 'excel-all': return window.excelAktarModal?.(true, []);
                case 'luca': return window.sendLucaToplu?.(uuids);
                case 'erp': return window.muhasebeyeTopluAktar?.(uuids);
                case 'print': return window.topluPrint?.(uuids);

                case 'dl-XML': return window.getDownloadFileAll?.('XML', uuids);
                case 'dl-PDF': return window.getDownloadFileAll?.('PDF', uuids);
                case 'dl-TPDF': return window.getDownloadFileAll?.('TPDF', uuids);
                case 'dl-HTML': return window.getDownloadFileAll?.('HTML', uuids);
                case 'dl-JPG': return window.getDownloadFileAll?.('JPG', uuids);
            }
        } catch (ex) {
            console.error('[OutboxEarchiveInvoice] bulk error:', ex);
            notifyErr(ex?.message || 'İşlem başarısız.');
        }
    });

    // ===== Global (View onclick’leri bozulmasın) =====
    window.invoicePreview = function (uuid) {
        if (!uuid) return;
        openApi('earchive/outbox/preview', { uuid });
    };

    window.topluPrint = function (uuids) {
        uuids = uuids || getSelectedUUIDs();
        if (!uuids.length) return notifyErr('Lütfen en az bir kayıt seçin.');
        openApi('earchive/outbox/print', { uuids: uuids.join(',') });
    };

    window.getDownloadFileAll = function (type, uuids) {
        uuids = uuids || getSelectedUUIDs();
        if (!uuids.length) return notifyErr('Lütfen en az bir kayıt seçin.');
        openApi('earchive/outbox/download', { type, uuids: uuids.join(',') });
    };

    // Modal açıcılar (modallar sayfada/partial’da varsa çalışır)
    window.openIptal = function (docNo, uuid) {
        $('#modalFaturaNoIptal').text(docNo || '');
        $('#modal-cancel').data('uuid', uuid).modal?.('show');
        $('#iptalEtBtn').off('click').on('click', () => window.sendCancelDocument(uuid));
    };

    window.openItiraz = function (docNo, uuid) {
        $('#modalFaturaNoItiraz').text(docNo || '');
        $('#modal-object').data('uuid', uuid).modal?.('show');
        $('#itirazEtBtn').off('click').on('click', () => window.sendObjectDocument(uuid));
    };

    window.sendCancelDocument = async function (uuid) {
        const dto = {
            UUID: uuid,
            CancelDate: toApiDate($('#CancelDate').val()),
            CancelReason: $('#CancelReason').val(),
            CancelMail: $('#CancelMail').val()
        };
        if (!dto.CancelDate || !dto.CancelReason) return notifyErr('Zorunlu alanları doldurunuz.');

        try {
            await EarchiveOutboxApi.cancel(dto);
            notifyOk('İptal talebi gönderildi.');
            $('#modal-cancel').modal?.('hide');
            gridSearch();
        } catch (ex) {
            notifyErr(ex?.message || 'İptal başarısız.');
        }
    };

    window.sendObjectDocument = async function (uuid) {
        const dto = {
            UUID: uuid,
            ObjectDocumentDate: toApiDate($('#ObjectDocumentDate').val()),
            ObjectDocumentNo: $('#ObjectDocumentNo').val(),
            ObjectType: $('#ObjectType').val(),
            ObjectReason: $('#ObjectReason').val()
        };
        if (!dto.ObjectDocumentDate || !dto.ObjectDocumentNo || !dto.ObjectType || !dto.ObjectReason) {
            return notifyErr('Zorunlu alanları doldurunuz.');
        }

        try {
            await EarchiveOutboxApi.object(dto);
            notifyOk('İtiraz talebi oluşturuldu.');
            $('#modal-object').modal?.('hide');
            gridSearch();
        } catch (ex) {
            notifyErr(ex?.message || 'İtiraz başarısız.');
        }
    };

    // Mail modal
    window.invoiceSendMailModal = function (uuid) {
        $('#modal-mail').data('uuid', uuid).modal?.('show');
    };
    window.invoiceSendMailModalCol = function (uuids) {
        $('#modal-mail').data('uuids', uuids || []).modal?.('show');
    };

    $('#sendMailBtn').on('click', async function () {
        const uuid = $('#modal-mail').data('uuid') || null;
        const uuids = $('#modal-mail').data('uuids') || (uuid ? [uuid] : []);
        const dto = {
            Uuids: uuids,
            ReceiverMail: $('#ReceiverMail').val(),
            Title: $('#Title').val(),
            ReceiverFirstnameLastname: $('#ReceiverFirstnameLastname').val(),
            AttachXml: $('#sendXml').is(':checked'),
            AttachPdf: $('#sendPdf').is(':checked')
        };
        if (!dto.ReceiverMail || !dto.Title || !dto.ReceiverFirstnameLastname) return notifyErr('E-posta, konu ve alıcı adı zorunlu.');

        try {
            await EarchiveOutboxApi.sendMail(dto);
            notifyOk('E-posta gönderildi.');
            $('#modal-mail').modal?.('hide');
        } catch (ex) {
            notifyErr(ex?.message || 'E-posta gönderilemedi.');
        }
    });

    // Excel modal
    window.excelAktarModal = function (all, uuids) {
        $('#modal-exceleaktar').data('all', !!all).data('uuids', uuids || []).modal?.('show');
    };

    $('#exceleAktar').on('click', async function () {
        const all = $('#modal-exceleaktar').data('all');
        const uuids = $('#modal-exceleaktar').data('uuids') || [];

        const dto = {
            All: !!all,
            Uuids: uuids,
            IssueStart: toApiDate($('#faturaBaslangicTarihi').val()),
            IssueEnd: toApiDate($('#faturaBitisTarihi').val()),
            ProcStart: toApiDate($('#faturaIslemeBaslamaTarihi').val()),
            ProcEnd: toApiDate($('#faturaIslemBitisTarihi').val()),
            IsArchive: $('#isArchive').val(),
            ReceiverIdentifier: $('#receiverIdentifier').val(),
            ReceiverTitle: $('#receiverTitle').val(),
            IsCancel: $('#isCancel').val(),
            Year: $('#IntegrationYearExcel').val(),
            SumKdv: $('#sumKdv').val()
        };

        try {
            const resp = await EarchiveOutboxApi.exportExcel(dto);
            notifyOk(resp?.message || 'Excel işlemi başlatıldı.');
            if (resp?.fileUrl) window.location = resp.fileUrl;
            $('#modal-exceleaktar').modal?.('hide');
        } catch (ex) {
            notifyErr(ex?.message || 'Excel aktarım hatası.');
        }
    });

    // Stub’lar (projedeki mevcut modallar/akışlar varsa burada değiştirirsin)
    window.sendLucaToplu = function (uuids) { notifyOk(`Luca aktarım isteği gönderildi (${uuids?.length || 0} kayıt).`); };
    window.muhasebeyeTopluAktar = function () { $('#muhasebeFisiModal').modal?.('show'); };

})();
