// wwwroot/apps/invoice.js
import { InvoiceApi } from '../Entites/index.js';

(function ($) {
    const APP_KEY = 'einvoice_draft_v1';
    const DEFAULT_UNIT_CODE = 'C62'; // ADET
    const DEFAULT_UNIT_NAME = 'ADET';
    const DEFAULT_CURRENCY = 'TRY';
    const DEFAULT_VAT = 20;

    const VAT_MODE = { EXCL: 'HARIC', INCL: 'DAHIL' };

    const log = (...a) => console.log(...a);
    const err = (...a) => console.error(...a);

    function dec(v) {
        v = ('' + (v ?? '')).replace(' TL', '').trim();
        v = v.replace(/\./g, '').replace(',', '.');
        const n = parseFloat(v);
        return Number.isFinite(n) ? n : 0;
    }
    function fmt(n) {
        n = (Math.round((n ?? 0) * 100) / 100).toFixed(2);
        const p = n.split('.');
        p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        return p.join(',');
    }
    function safeInitPickers() {
        try {
            if ($.fn.datepicker) $('.datepicker').datepicker({ language: 'tr', autoclose: true, todayHighlight: true });
            if ($.fn.timepicker) $('.timepicker').timepicker({ showMeridian: false, minuteStep: 1, defaultTime: false });
        } catch { }
    }
    function getVatMode() {
        const x = ($('#vatMode').val() || '').toUpperCase();
        return x.indexOf('DAH') >= 0 ? VAT_MODE.INCL : VAT_MODE.EXCL;
    }

    function rowTemplate(i) {
        return `
      <tr>
        <td class="text-center">${i}</td>
        <td><input class="form-control ln-ad" placeholder="Mal/Hizmet Adı" value="GENEL ÜRÜN"/></td>
        <td><input type="number" min="0" step="0.0001" class="form-control ln-qty" value="1"/></td>
        <td>
          <input class="form-control ln-unit" value="${DEFAULT_UNIT_CODE}" title="C62=ADET"/>
        </td>
        <td><input type="number" step="0.01" class="form-control ln-price" value="1"/></td>
        <td><input type="number" step="0.01" class="form-control ln-discp" value="0"/></td>
        <td class="hidden-xs"><input class="form-control ln-isk" readonly value="0,00"/></td>
        <td class="hidden-xs"><input class="form-control ln-net" readonly value="1,00"/></td>
        <td><input type="number" step="0.01" class="form-control ln-kdv" value="${DEFAULT_VAT}"/></td>
        <td class="hidden-xs"><input class="form-control ln-kdvt" readonly value="0,20"/></td>
        <td class="hidden-sm hidden-xs"><input class="form-control ln-istisna" placeholder="İstisna"/></td>
        <td class="text-center"><button class="btn btn-xs btn-danger btnDel"><i class="fa fa-trash"></i></button></td>
      </tr>
    `;
    }
    function renumber() {
        $('#tblLines tbody tr').each(function (i) {
            $(this).find('td:first').text(i + 1);
        });
    }

    function recalc() {
        const vatMode = getVatMode();
        let ara = 0, isk = 0, netSum = 0, kdvSum = 0, genel = 0;

        $('#tblLines tbody tr').each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            const rate = dec($r.find('.ln-kdv').val()) || 0;

            let tutar = qty * price;
            if (vatMode === VAT_MODE.INCL) {
                const unitNet = price / (1 + rate / 100);
                tutar = qty * unitNet;
            }
            const iskT = tutar * (discp / 100);
            const base = tutar - iskT;
            const kdvt = base * (rate / 100);
            const gross = base + kdvt;

            if ($r.find('.ln-isk').length) $r.find('.ln-isk').val(fmt(iskT));
            if ($r.find('.ln-net').length) $r.find('.ln-net').val(fmt(base));
            if ($r.find('.ln-kdvt').length) $r.find('.ln-kdvt').val(fmt(kdvt));
            if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(gross));

            ara += qty * price;
            isk += iskT;
            netSum += base;
            kdvSum += kdvt;
            genel += gross;
        });

        if ($('#tAra').length) $('#tAra').text(fmt(ara) + ' TL');
        if ($('#tIsk').length) $('#tIsk').text(fmt(isk) + ' TL');
        if ($('#tMatrah20').length) $('#tMatrah20').text(fmt(netSum) + ' TL');
        if ($('#tKdv20').length) $('#tKdv20').text(fmt(kdvSum) + ' TL');
        $('#tGenel').text(fmt(genel) + ' TL');
    }

    function computeTaxes() {
        const vatMode = getVatMode();
        const bag = {}; // rate -> { base, amount }
        $('#tblLines tbody tr').each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            const rate = dec($r.find('.ln-kdv').val()) || 0;

            let tutar = qty * price;
            if (vatMode === VAT_MODE.INCL) {
                const unitNet = price / (1 + rate / 100);
                tutar = qty * unitNet;
            }
            const iskT = tutar * (discp / 100);
            const base = tutar - iskT;
            const amount = base * (rate / 100);

            if (!bag[rate]) bag[rate] = { base: 0, amount: 0 };
            bag[rate].base += base;
            bag[rate].amount += amount;
        });
        return bag;
    }

    function saveDraft() {
        try {
            const dto = collectInvoice(false);
            localStorage.setItem(APP_KEY, JSON.stringify(dto));
        } catch { }
    }
    function loadDraft() {
        try {
            const raw = localStorage.getItem(APP_KEY);
            if (!raw) return;
            const dto = JSON.parse(raw);
            applyDraft(dto);
        } catch { }
    }
    function clearDraft() { try { localStorage.removeItem(APP_KEY); } catch { } }

    function collectInvoice(willSend = true) {
        const now = new Date().toISOString();
        const invoiceNo = 'INV-' + Date.now();
        const currency = (($('#ddlParaBirimi').val() || DEFAULT_CURRENCY) + '').toUpperCase();
        const total = dec($('#tGenel').text());

        const dto = {
            entity: 'EInvoice',
            invoiceNo,
            invoiceDate: now,
            currency,
            total,
            createdAt: now,
            updatedAt: now,

            customer: {
                name: $('#CustomerName').val() || 'Varsayılan Müşteri',
                surname: $('#CustomerSurname').val() || '',
                phone: '',
                email: $('#CustomerEmail').val() || '',
                taxNo: $('#CustomerTaxNo').val() || '',
                taxOffice: $('#CustomerTaxOffice').val() || '',
                createdAt: now,
                updatedAt: now,
                customersGroups: [],
                addresses: [],
                invoices: []
            },

            invoicesItems: [],
            invoicesTaxes: [],
            invoicesDiscounts: [],
            sgkRecords: [],
            invoicesPayments: [],
            servicesProviders: [],
            returns: [],
            tourists: []
        };

        // Kalemler
        $('#tblLines tbody tr').each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            const rate = dec($r.find('.ln-kdv').val()) || 0;

            const unitShort = ($r.find('.ln-unit').val() || DEFAULT_UNIT_CODE).toUpperCase();
            const unitName = (unitShort === 'C62' ? DEFAULT_UNIT_NAME : unitShort);

            // satır toplamı (varsa .ln-total’dan, yoksa hesapla)
            const gross = $r.find('.ln-total').length
                ? dec($r.find('.ln-total').val())
                : (function () {
                    let tutar = qty * price;
                    if (getVatMode() === VAT_MODE.INCL) {
                        const unitNet = price / (1 + rate / 100);
                        tutar = qty * unitNet;
                    }
                    const base = tutar - (tutar * (discp / 100));
                    return base * (1 + rate / 100);
                })();

            dto.invoicesItems.push({
                quantity: qty,
                price: price,
                total: gross,
                createdAt: now,
                updatedAt: now,
                // Birimi navigation olarak gönderiyoruz; iş katmanı ShortName/Name’den bulur veya oluşturur.
                item: {
                    name: $r.find('.ln-ad').val() || 'GENEL ÜRÜN',
                    code: 'ITEM-' + Math.floor(Math.random() * 100000),
                    currency: currency,
                    createdAt: now,
                    updatedAt: now,
                    unit: {
                        shortName: unitShort,
                        name: unitName,
                        createdAt: now,
                        updatedAt: now
                    }
                }
            });
        });

        // Vergiler (oranlara göre)
        const taxes = computeTaxes();
        Object.keys(taxes).forEach(k => {
            const rate = Number(k);
            dto.invoicesTaxes.push({
                name: 'KDV',
                rate: rate,
                amount: taxes[k].amount,
                createdAt: now,
                updatedAt: now
            });
        });

        // Genel iskonto (satırlardan)
        let totalDisc = 0;
        $('#tblLines tbody tr').each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            const rate = dec($r.find('.ln-kdv').val()) || 0;
            let tutar = qty * price;
            if (getVatMode() === VAT_MODE.INCL) {
                const unitNet = price / (1 + rate / 100);
                tutar = qty * unitNet;
            }
            totalDisc += tutar * (discp / 100);
        });
        dto.invoicesDiscounts.push({
            name: 'Toplam İskonto',
            desc: 'Otomatik',
            base: 'Ara Toplam',
            rate: 0,
            amount: totalDisc,
            createdAt: now,
            updatedAt: now
        });

        // SGK (varsa)
        const sgkProv = $('#SgkProvizyon').val();
        const sgkTakip = $('#SgkTakip').val();
        if (sgkProv || sgkTakip) {
            dto.sgkRecords.push({
                type: 'SGK',
                code: 'SGK001',
                name: 'Provizyon Takip',
                no: sgkTakip || '0',
                startDate: now,
                endDate: now,
                createdAt: now,
                updatedAt: now
            });
        }

        // Ödeme (nav ile name/swiftCode)
        dto.invoicesPayments.push({
            createdAt: now,
            updatedAt: now,
            payment: {
                amount: dto.total,
                currency: currency,
                date: now,
                note: 'Nakit ödeme',
                createdAt: now,
                updatedAt: now,
                paymentType: { name: 'NAKIT', createdAt: now, updatedAt: now },
                paymentAccount: { name: 'KASA', createdAt: now, updatedAt: now },
                // Bankayı istersen swift veya adla gönderebilirsin:
                bank: { name: 'Ziraat Bankası', /* swiftCode: 'TCZBTR2A',*/ createdAt: now, updatedAt: now }
            }
        });

        // Servis sağlayıcı
        dto.servicesProviders.push({
            no: 'SP-001',
            systemUser: 'UI',
            createdAt: now,
            updatedAt: now
        });

        return dto;
    }

    function applyDraft(dto) {
        try {
            if (dto.customer) {
                $('#CustomerName').val(dto.customer.name || '');
                $('#CustomerSurname').val(dto.customer.surname || '');
                $('#CustomerEmail').val(dto.customer.email || '');
                $('#CustomerTaxNo').val(dto.customer.taxNo || '');
                $('#CustomerTaxOffice').val(dto.customer.taxOffice || '');
            }
            if (dto.currency) $('#ddlParaBirimi').val(dto.currency);

            $('#tblLines tbody').empty();
            const items = dto.invoicesItems || [];
            if (items.length === 0) {
                $('#btnAddRow').click();
            } else {
                items.forEach((x, i) => {
                    $('#tblLines tbody').append(rowTemplate(i + 1));
                    const $r = $('#tblLines tbody tr').last();
                    $r.find('.ln-ad').val(x.item?.name || 'GENEL ÜRÜN');
                    $r.find('.ln-qty').val(x.quantity ?? 1);
                    $r.find('.ln-unit').val(x.item?.unit?.shortName || DEFAULT_UNIT_CODE);
                    $r.find('.ln-price').val(x.price ?? 1);
                    $r.find('.ln-discp').val(0);
                    $r.find('.ln-kdv').val((dto.invoicesTaxes?.[0]?.rate) ?? DEFAULT_VAT);
                    if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(x.total ?? 0));
                });
            }
            recalc();
        } catch { clearDraft(); }
    }

    function bindPreview() {
        const $m = $('#modal-onizleme');
        if ($m.length === 0) return;
        $m.on('show.bs.modal', function () {
            let html = '<html><head><meta charset="utf-8"><title>Önizleme</title>';
            html += '<style>body{font-family:Arial;padding:14px}table{width:100%;border-collapse:collapse}td,th{border:1px solid #ccc;padding:6px;font-size:12px}</style>';
            html += '</head><body>';
            html += '<h3>e-Fatura Önizleme</h3>';
            html += document.getElementById('tblLines').outerHTML;
            html += `<p><b>Genel Toplam:</b> ${$('#tGenel').text()}</p>`;
            html += '</body></html>';
            const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
            $('#onizle-iframe').attr('src', url);
        });
    }

    async function doSave() {
        const dto = collectInvoice(true);
        log('📦 Gönderilecek DTO:', dto);
        if (!dto.invoicesItems?.length) { toastr.error('En az bir satır ekleyin.'); return; }
        try {
            const res = await InvoiceApi.create(dto);
            toastr.success('Fatura başarıyla kaydedildi.');
            log('✅ API Yanıtı:', res);
            clearDraft();
        } catch (e) {
            const msg = e?.message || ('' + e);
            err('❌ Hata:', e);
            toastr.error('Fatura kaydedilemedi: ' + msg);
        }
    }

    $(function () {
        console.log('🧾 e-Fatura sayfası yüklendi.');
        safeInitPickers();
        bindPreview();

        $('#btnAddRow').off('click').on('click', function () {
            const idx = $('#tblLines tbody tr').length + 1;
            $('#tblLines tbody').append(rowTemplate(idx));
            recalc();
            saveDraft();
        });

        $(document).on('click', '.btnDel', function () {
            $(this).closest('tr').remove();
            renumber();
            recalc();
            saveDraft();
        });

        $(document).on('input change', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
            recalc();
            saveDraft();
        });

        $('#vatMode').on('change', function () {
            recalc();
            saveDraft();
        });

        $('#btnKaydet, #btnSendGib').off('click').on('click', async function () {
            await doSave();
        });

        loadDraft();
        if ($('#tblLines tbody tr').length === 0) $('#btnAddRow').click();
        setInterval(saveDraft, 10000);
    });
})(jQuery);
