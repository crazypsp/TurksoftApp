//// wwwroot/apps/invoice.js
import { InvoiceApi } from '../Entites/index.js';

//(function ($) {
//    const APP_KEY = 'einvoice_draft_v1';
//    const DEFAULT_UNIT_CODE = 'C62'; // ADET
//    const DEFAULT_UNIT_NAME = 'ADET';
//    const DEFAULT_CURRENCY = 'TRY';
//    const DEFAULT_VAT = 20;

//    const VAT_MODE = { EXCL: 'HARIC', INCL: 'DAHIL' };

//    const log = (...a) => console.log(...a);
//    const err = (...a) => console.error(...a);

//    function dec(v) {
//        v = ('' + (v ?? '')).replace(' TL', '').trim();
//        v = v.replace(/\./g, '').replace(',', '.');
//        const n = parseFloat(v);
//        return Number.isFinite(n) ? n : 0;
//    }
//    function fmt(n) {
//        n = (Math.round((n ?? 0) * 100) / 100).toFixed(2);
//        const p = n.split('.');
//        p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
//        return p.join(',');
//    }
//    function safeInitPickers() {
//        try {
//            if ($.fn.datepicker) $('.datepicker').datepicker({ language: 'tr', autoclose: true, todayHighlight: true });
//            if ($.fn.timepicker) $('.timepicker').timepicker({ showMeridian: false, minuteStep: 1, defaultTime: false });
//        } catch { }
//    }
//    function getVatMode() {
//        const x = ($('#vatMode').val() || '').toUpperCase();
//        return x.indexOf('DAH') >= 0 ? VAT_MODE.INCL : VAT_MODE.EXCL;
//    }

//    function rowTemplate(i) {
//        return `
//      <tr>
//        <td class="text-center">${i}</td>
//        <td><input class="form-control ln-ad" placeholder="Mal/Hizmet Adı" value="GENEL ÜRÜN"/></td>
//        <td><input type="number" min="0" step="0.0001" class="form-control ln-qty" value="1"/></td>
//        <td>
//          <input class="form-control ln-unit" value="${DEFAULT_UNIT_CODE}" title="C62=ADET"/>
//        </td>
//        <td><input type="number" step="0.01" class="form-control ln-price" value="1"/></td>
//        <td><input type="number" step="0.01" class="form-control ln-discp" value="0"/></td>
//        <td class="hidden-xs"><input class="form-control ln-isk" readonly value="0,00"/></td>
//        <td class="hidden-xs"><input class="form-control ln-net" readonly value="1,00"/></td>
//        <td><input type="number" step="0.01" class="form-control ln-kdv" value="${DEFAULT_VAT}"/></td>
//        <td class="hidden-xs"><input class="form-control ln-kdvt" readonly value="0,20"/></td>
//        <td class="hidden-sm hidden-xs"><input class="form-control ln-istisna" placeholder="İstisna"/></td>
//        <td class="text-center"><button class="btn btn-xs btn-danger btnDel"><i class="fa fa-trash"></i></button></td>
//      </tr>
//    `;
//    }
//    function renumber() {
//        $('#tblLines tbody tr').each(function (i) {
//            $(this).find('td:first').text(i + 1);
//        });
//    }

//    function recalc() {
//        const vatMode = getVatMode();
//        let ara = 0, isk = 0, netSum = 0, kdvSum = 0, genel = 0;

//        $('#tblLines tbody tr').each(function () {
//            const $r = $(this);
//            const qty = dec($r.find('.ln-qty').val());
//            const price = dec($r.find('.ln-price').val());
//            const discp = dec($r.find('.ln-discp').val());
//            const rate = dec($r.find('.ln-kdv').val()) || 0;

//            let tutar = qty * price;
//            if (vatMode === VAT_MODE.INCL) {
//                const unitNet = price / (1 + rate / 100);
//                tutar = qty * unitNet;
//            }
//            const iskT = tutar * (discp / 100);
//            const base = tutar - iskT;
//            const kdvt = base * (rate / 100);
//            const gross = base + kdvt;

//            if ($r.find('.ln-isk').length) $r.find('.ln-isk').val(fmt(iskT));
//            if ($r.find('.ln-net').length) $r.find('.ln-net').val(fmt(base));
//            if ($r.find('.ln-kdvt').length) $r.find('.ln-kdvt').val(fmt(kdvt));
//            if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(gross));

//            ara += qty * price;
//            isk += iskT;
//            netSum += base;
//            kdvSum += kdvt;
//            genel += gross;
//        });

//        if ($('#tAra').length) $('#tAra').text(fmt(ara) + ' TL');
//        if ($('#tIsk').length) $('#tIsk').text(fmt(isk) + ' TL');
//        if ($('#tMatrah20').length) $('#tMatrah20').text(fmt(netSum) + ' TL');
//        if ($('#tKdv20').length) $('#tKdv20').text(fmt(kdvSum) + ' TL');
//        $('#tGenel').text(fmt(genel) + ' TL');
//    }

//    function computeTaxes() {
//        const vatMode = getVatMode();
//        const bag = {}; // rate -> { base, amount }
//        $('#tblLines tbody tr').each(function () {
//            const $r = $(this);
//            const qty = dec($r.find('.ln-qty').val());
//            const price = dec($r.find('.ln-price').val());
//            const discp = dec($r.find('.ln-discp').val());
//            const rate = dec($r.find('.ln-kdv').val()) || 0;

//            let tutar = qty * price;
//            if (vatMode === VAT_MODE.INCL) {
//                const unitNet = price / (1 + rate / 100);
//                tutar = qty * unitNet;
//            }
//            const iskT = tutar * (discp / 100);
//            const base = tutar - iskT;
//            const amount = base * (rate / 100);

//            if (!bag[rate]) bag[rate] = { base: 0, amount: 0 };
//            bag[rate].base += base;
//            bag[rate].amount += amount;
//        });
//        return bag;
//    }

//    function saveDraft() {
//        try {
//            const dto = collectInvoice(false);
//            localStorage.setItem(APP_KEY, JSON.stringify(dto));
//        } catch { }
//    }
//    function loadDraft() {
//        try {
//            const raw = localStorage.getItem(APP_KEY);
//            if (!raw) return;
//            const dto = JSON.parse(raw);
//            applyDraft(dto);
//        } catch { }
//    }
//    function clearDraft() { try { localStorage.removeItem(APP_KEY); } catch { } }

//    function collectInvoice(willSend = true) {
//        const now = new Date().toISOString();
//        const invoiceNo = 'INV-' + Date.now();
//        const currency = (($('#ddlParaBirimi').val() || DEFAULT_CURRENCY) + '').toUpperCase();
//        const total = dec($('#tGenel').text());

//        const dto = {
//            entity: 'EInvoice',
//            invoiceNo,
//            invoiceDate: now,
//            currency,
//            total,
//            createdAt: now,
//            updatedAt: now,

//            customer: {
//                name: $('#CustomerName').val() || 'Varsayılan Müşteri',
//                surname: $('#CustomerSurname').val() || '',
//                phone: '',
//                email: $('#CustomerEmail').val() || '',
//                taxNo: $('#CustomerTaxNo').val() || '',
//                taxOffice: $('#CustomerTaxOffice').val() || '',
//                createdAt: now,
//                updatedAt: now,
//                customersGroups: [],
//                addresses: [],
//                invoices: []
//            },

//            invoicesItems: [],
//            invoicesTaxes: [],
//            invoicesDiscounts: [],
//            sgkRecords: [],
//            invoicesPayments: [],
//            servicesProviders: [],
//            returns: [],
//            tourists: []
//        };

//        // Kalemler
//        $('#tblLines tbody tr').each(function () {
//            const $r = $(this);
//            const qty = dec($r.find('.ln-qty').val());
//            const price = dec($r.find('.ln-price').val());
//            const discp = dec($r.find('.ln-discp').val());
//            const rate = dec($r.find('.ln-kdv').val()) || 0;

//            const unitShort = ($r.find('.ln-unit').val() || DEFAULT_UNIT_CODE).toUpperCase();
//            const unitName = (unitShort === 'C62' ? DEFAULT_UNIT_NAME : unitShort);

//            // satır toplamı (varsa .ln-total’dan, yoksa hesapla)
//            const gross = $r.find('.ln-total').length
//                ? dec($r.find('.ln-total').val())
//                : (function () {
//                    let tutar = qty * price;
//                    if (getVatMode() === VAT_MODE.INCL) {
//                        const unitNet = price / (1 + rate / 100);
//                        tutar = qty * unitNet;
//                    }
//                    const base = tutar - (tutar * (discp / 100));
//                    return base * (1 + rate / 100);
//                })();

//            dto.invoicesItems.push({
//                quantity: qty,
//                price: price,
//                total: gross,
//                createdAt: now,
//                updatedAt: now,
//                // Birimi navigation olarak gönderiyoruz; iş katmanı ShortName/Name’den bulur veya oluşturur.
//                item: {
//                    name: $r.find('.ln-ad').val() || 'GENEL ÜRÜN',
//                    code: 'ITEM-' + Math.floor(Math.random() * 100000),
//                    currency: currency,
//                    createdAt: now,
//                    updatedAt: now,
//                    unit: {
//                        shortName: unitShort,
//                        name: unitName,
//                        createdAt: now,
//                        updatedAt: now
//                    }
//                }
//            });
//        });

//        // Vergiler (oranlara göre)
//        const taxes = computeTaxes();
//        Object.keys(taxes).forEach(k => {
//            const rate = Number(k);
//            dto.invoicesTaxes.push({
//                name: 'KDV',
//                rate: rate,
//                amount: taxes[k].amount,
//                createdAt: now,
//                updatedAt: now
//            });
//        });

//        // Genel iskonto (satırlardan)
//        let totalDisc = 0;
//        $('#tblLines tbody tr').each(function () {
//            const $r = $(this);
//            const qty = dec($r.find('.ln-qty').val());
//            const price = dec($r.find('.ln-price').val());
//            const discp = dec($r.find('.ln-discp').val());
//            const rate = dec($r.find('.ln-kdv').val()) || 0;
//            let tutar = qty * price;
//            if (getVatMode() === VAT_MODE.INCL) {
//                const unitNet = price / (1 + rate / 100);
//                tutar = qty * unitNet;
//            }
//            totalDisc += tutar * (discp / 100);
//        });
//        dto.invoicesDiscounts.push({
//            name: 'Toplam İskonto',
//            desc: 'Otomatik',
//            base: 'Ara Toplam',
//            rate: 0,
//            amount: totalDisc,
//            createdAt: now,
//            updatedAt: now
//        });

//        // SGK (varsa)
//        const sgkProv = $('#SgkProvizyon').val();
//        const sgkTakip = $('#SgkTakip').val();
//        if (sgkProv || sgkTakip) {
//            dto.sgkRecords.push({
//                type: 'SGK',
//                code: 'SGK001',
//                name: 'Provizyon Takip',
//                no: sgkTakip || '0',
//                startDate: now,
//                endDate: now,
//                createdAt: now,
//                updatedAt: now
//            });
//        }

//        // Ödeme (nav ile name/swiftCode)
//        dto.invoicesPayments.push({
//            createdAt: now,
//            updatedAt: now,
//            payment: {
//                amount: dto.total,
//                currency: currency,
//                date: now,
//                note: 'Nakit ödeme',
//                createdAt: now,
//                updatedAt: now,
//                paymentType: { name: 'NAKIT', createdAt: now, updatedAt: now },
//                paymentAccount: { name: 'KASA', createdAt: now, updatedAt: now },
//                // Bankayı istersen swift veya adla gönderebilirsin:
//                bank: { name: 'Ziraat Bankası', /* swiftCode: 'TCZBTR2A',*/ createdAt: now, updatedAt: now }
//            }
//        });

//        // Servis sağlayıcı
//        dto.servicesProviders.push({
//            no: 'SP-001',
//            systemUser: 'UI',
//            createdAt: now,
//            updatedAt: now
//        });

//        return dto;
//    }

//    function applyDraft(dto) {
//        try {
//            if (dto.customer) {
//                $('#CustomerName').val(dto.customer.name || '');
//                $('#CustomerSurname').val(dto.customer.surname || '');
//                $('#CustomerEmail').val(dto.customer.email || '');
//                $('#CustomerTaxNo').val(dto.customer.taxNo || '');
//                $('#CustomerTaxOffice').val(dto.customer.taxOffice || '');
//            }
//            if (dto.currency) $('#ddlParaBirimi').val(dto.currency);

//            $('#tblLines tbody').empty();
//            const items = dto.invoicesItems || [];
//            if (items.length === 0) {
//                $('#btnAddRow').click();
//            } else {
//                items.forEach((x, i) => {
//                    $('#tblLines tbody').append(rowTemplate(i + 1));
//                    const $r = $('#tblLines tbody tr').last();
//                    $r.find('.ln-ad').val(x.item?.name || 'GENEL ÜRÜN');
//                    $r.find('.ln-qty').val(x.quantity ?? 1);
//                    $r.find('.ln-unit').val(x.item?.unit?.shortName || DEFAULT_UNIT_CODE);
//                    $r.find('.ln-price').val(x.price ?? 1);
//                    $r.find('.ln-discp').val(0);
//                    $r.find('.ln-kdv').val((dto.invoicesTaxes?.[0]?.rate) ?? DEFAULT_VAT);
//                    if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(x.total ?? 0));
//                });
//            }
//            recalc();
//        } catch { clearDraft(); }
//    }

//    function bindPreview() {
//        const $m = $('#modal-onizleme');
//        if ($m.length === 0) return;
//        $m.on('show.bs.modal', function () {
//            let html = '<html><head><meta charset="utf-8"><title>Önizleme</title>';
//            html += '<style>body{font-family:Arial;padding:14px}table{width:100%;border-collapse:collapse}td,th{border:1px solid #ccc;padding:6px;font-size:12px}</style>';
//            html += '</head><body>';
//            html += '<h3>e-Fatura Önizleme</h3>';
//            html += document.getElementById('tblLines').outerHTML;
//            html += `<p><b>Genel Toplam:</b> ${$('#tGenel').text()}</p>`;
//            html += '</body></html>';
//            const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
//            $('#onizle-iframe').attr('src', url);
//        });
//    }

//    async function doSave() {
//        const dto = collectInvoice(true);
//        log('📦 Gönderilecek DTO:', dto);
//        if (!dto.invoicesItems?.length) { toastr.error('En az bir satır ekleyin.'); return; }
//        try {
//            const res = await InvoiceApi.create(dto);
//            toastr.success('Fatura başarıyla kaydedildi.');
//            log('✅ API Yanıtı:', res);
//            clearDraft();
//        } catch (e) {
//            const msg = e?.message || ('' + e);
//            err('❌ Hata:', e);
//            toastr.error('Fatura kaydedilemedi: ' + msg);
//        }
//    }

//    $(function () {
//        console.log('🧾 e-Fatura sayfası yüklendi.');
//        safeInitPickers();
//        bindPreview();

//        $('#btnAddRow').off('click').on('click', function () {
//            const idx = $('#tblLines tbody tr').length + 1;
//            $('#tblLines tbody').append(rowTemplate(idx));
//            recalc();
//            saveDraft();
//        });

//        $(document).on('click', '.btnDel', function () {
//            $(this).closest('tr').remove();
//            renumber();
//            recalc();
//            saveDraft();
//        });

//        $(document).on('input change', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
//            recalc();
//            saveDraft();
//        });

//        $('#vatMode').on('change', function () {
//            recalc();
//            saveDraft();
//        });

//        $('#btnKaydet, #btnSendGib').off('click').on('click', async function () {
//            await doSave();
//        });

//        loadDraft();
//        if ($('#tblLines tbody tr').length === 0) $('#btnAddRow').click();
//        setInterval(saveDraft, 10000);
//    });
//})(jQuery);




// wwwroot/apps/invoice.jquery.js
// jQuery implementation (no Angular).
// Requirements implemented:
// 1) Faithful DOM wiring to original HTML (ids preserved)
// 2) Dropdowns filled via GET lookups
// 3) 'Taslak Kaydet' triggers InvoiceApi.create(dto) like doSave()
// 4) + buttons add new rows to tables; delete buttons remove rows
// 5) All original selects populated
// 6) DTO conforms to Invoice.cs and related entities


// wwwroot/apps/invoice.jquery.js
/* wwwroot/apps/invoice.full.js
 * jQuery only • Static lists • SGK dynamic • Lines add/remove • Live totals • InvoiceApi integration
 * (c) you — single file orchestrator
*/
// wwwroot/apps/invoice.js (FINAL - full jQuery, drop-in compatible)
// Özellikler:
// - Yeni Satır Ekle: lines(#lines/#tblLines/#manuel_grid), irsaliye_grid, saticiekalan_grid, saticiAgentekalan_grid, aliciekalan_grid
// - Canlı toplamlar, taslak kaydet/yükle
// - Önizleme: modalPreview -> modal-onizleme -> yeni sekme
// - PDF/XML indir
// - API: InvoiceApi (global varsa) / REST fallback
// - "Nox Yazılım" gibi alt yazıları gizler
// wwwroot/apps/invoice.js (FINAL — patched)
// Özellikler:
// - Yeni Satır Ekle: lines (#manuel_grid öncelikli), irsaliye_grid, saticiekalan_grid, saticiAgentekalan_grid, aliciekalan_grid
// - Canlı toplamlar, taslak kaydet/yükle
// - Önizleme: modalPreview -> modal-onizleme -> yeni sekme
// - PDF/XML indir
// - API: InvoiceApi (global varsa) / REST fallback
// - "Nox Yazılım" gibi alt yazıları gizler
(function ($, w, d) {
    'use strict';

    // ===========================
    // SEÇİCİLER
    // ===========================
    const SEL = {
        // Üst alanlar
        subeKodu: '#subeKodu',
        prefix: '#txtInvoice_Prefix',
        sourceUrn: '#SourceUrn',
        destUrn: '#DestinationUrn',
        xslt: '#ddlxsltdosyasi',
        currency: '#DocumentCurrencyCode',

        // Müşteri
        custName: '#txtPartyName',
        custSurname: '#txtPerson_FamilyName',
        custMail: '#txtElectronicMail',
        custTaxNo: '#txtIdentificationID',
        custTaxOffice: '#txtTaxSchemeName',

        // Adres & ülke/il/ilçe
        country: '#selulke',
        city: '#selIl',
        district: '#selIlce',
        txtIl: '#txtIl',
        txtIlce: '#txtIlce',

        // Ödeme
        payMeans: '#PaymentMeansCode',
        payChannel: '#PaymentChannelCode',
        payNote: '#InstructionNote',
        payAccount: '#PayeeFinancialAccount',
        bankName: '#txtBankName',

        // SGK
        sgkType: '#ddlilaveFaturaTipi',
        sgkMukKod: '#mukkodack',
        sgkMukAdi: '#mukadiack',
        sgkDosya: '#dosyaack',
        sgkStart: '#txtSendingDateBaslangic',
        sgkEnd: '#txtSendingDateBitis',

        // Kalem tablo (başlangıçta varsayılan; DOM ready’de manuel_grid önceliği verilecek)
        linesTable: '#tblLines',

        // Diğer gridler
        despatchTable: '#irsaliye_grid',
        sellerExtraTbl: '#saticiekalan_grid',
        sellerAgentTbl: '#saticiAgentekalan_grid',
        buyerExtraTbl: '#aliciekalan_grid',

        // Butonlar
        btnAddLine: '#btnAddRow, #btnNewLine, .btn-new-line',
        btnAddByText: 'button,a', // metninde “Yeni Satır Ekle” olanlar
        btnDespatchAdd: '#btnDespatchAdd',
        btnSellerExtra: '#btnSellerExtraAdd',
        btnSellerAgent: '#btnSellerAgentExtraAdd',
        btnBuyerExtra: '#btnBuyerExtraAdd',
        btnSave: '#btn_taslak, [name="btn_taslak"], #btnKaydet, #btnDraftSave',
        btnSend: '#btnSendGib, #btnGibSend',
        btnPreview: '#btnPreview, #btnOnizleme',
        btnPdf: '#btnPdfDownload, #btnPdf',
        btnXml: '#btnXmlDownload, #btnXml',

        // Toplam alanları
        tAra: '#tAra',
        tIsk: '#tIsk',
        tMatrah20: '#tMatrah20',
        tKdv20: '#tKdv20',
        tGenel: '#tGenel'
    };

    // ===========================
    // SABİTLER
    // ===========================
    const DEFAULTS = {
        UNIT_CODE: 'C62',
        UNIT_NAME: 'ADET',
        VAT: 20,
        CURRENCY: 'TRY',
        APP_KEY: 'einvoice_draft_full_v1'
    };
    const VAT_MODE = { EXCL: 'HARIC', INCL: 'DAHIL' };

    // ===========================
    // YARDIMCI FONKSİYONLAR
    // ===========================
    const log = (...a) => console.log(...a);
    const err = (...a) => console.error(...a);

    function fmt(n) {
        n = (Math.round((n ?? 0) * 100) / 100).toFixed(2);
        const p = n.split('.');
        p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        return p.join(',');
    }
    function dec(v) {
        v = ('' + (v ?? '')).replace(' TL', '').trim().replace(/\./g, '').replace(',', '.');
        const n = parseFloat(v); return Number.isFinite(n) ? n : 0;
    }
    function setVal(sel, val) {
        const $el = $(sel); if (!$el.length) return;
        if ($el.is('input,select,textarea')) $el.val(val); else $el.text(val);
    }
    function fillSelect($el, list, getVal, getText, placeholder) {
        if (!$el || !$el.length) return;
        $el.empty();
        if (placeholder) $el.append($('<option/>', { value: '', text: placeholder }));
        (list || []).forEach(x => $el.append($('<option/>', { value: getVal(x), text: getText(x) })));
        $el.trigger('change');
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
    function clearInputs($scope) {
        $scope.find('input,select,textarea').each(function () {
            const $el = $(this);
            const type = ($el.attr('type') || '').toLowerCase();
            if (type === 'checkbox' || type === 'radio') $el.prop('checked', false);
            else $el.val('');
        });
    }

    // ===========================
    // LOOKUP DOLUM
    // ===========================
    function loadLookups() {
        if (w.subeList) fillSelect($(SEL.subeKodu), subeList, x => x.SubeKodu, x => x.SubeAdi, (subeList?.length > 1 ? 'Seçiniz' : ''));

        const prefixList = [];
        try { if (w.invoicemodel?.invoiceheader?.Prefix) prefixList.push(w.invoicemodel.invoiceheader.Prefix); } catch { }
        ['INV', 'TS', 'NYZ'].forEach(x => { if (!prefixList.includes(x)) prefixList.push(x); });
        fillSelect($(SEL.prefix), prefixList, x => x, x => x, 'Seçiniz');

        if (w.GondericiEtiketList) fillSelect($(SEL.sourceUrn), GondericiEtiketList, x => x, x => x, 'Seçiniz');
        if (w.KurumEtiketList) fillSelect($(SEL.destUrn), KurumEtiketList, x => x, x => x, 'Seçiniz');

        if (w.GetXsltList) fillSelect($(SEL.xslt), GetXsltList, x => x, x => x, 'Seçiniz');

        if (w.parabirimList) {
            fillSelect($(SEL.currency), parabirimList, x => x.Kodu, x => `${x.Kodu} — ${x.Aciklama}`, 'Seçiniz');
            if (!$(SEL.currency).val()) $(SEL.currency).val(DEFAULTS.CURRENCY).trigger('change');
        } else {
            fillSelect($(SEL.currency), [{ Kodu: 'TRY', Aciklama: 'Türk Lirası' }], x => x.Kodu, x => `${x.Kodu} — ${x.Aciklama}`, 'Seçiniz');
            $(SEL.currency).val('TRY').trigger('change');
        }

        if (w.ulkeList) fillSelect($(SEL.country), ulkeList, x => x.UlkeKodu || x.UlkeAdi, x => x.UlkeAdi, 'Seçiniz');
        if (w.ilList) fillSelect($(SEL.city), ilList, x => x.IlAdi, x => x.IlAdi, 'Seçiniz');
        $(SEL.city).off('change.district').on('change.district', function () {
            const city = $(this).val();
            if (w.ilceList) {
                const rows = ilceList.filter(x => x.IlAdi === city);
                fillSelect($(SEL.district), rows, x => x.IlceAdi, x => x.IlceAdi, 'Seçiniz');
            } else {
                if ($(SEL.txtIlce).length) { $(SEL.district).hide(); $(SEL.txtIlce).show().val(''); }
            }
        });

        if (w.OdemeList) fillSelect($(SEL.payMeans), OdemeList, x => x.OdemeKodu, x => `${x.OdemeKodu} — ${x.Aciklama}`, 'Seçiniz');
        if (w.OdemeKanalList) fillSelect($(SEL.payChannel), OdemeKanalList, x => x.OdemeKanalKodu, x => `${x.OdemeKanalKodu} — ${x.Aciklama}`, 'Seçiniz');

        w.__unitList = (w.birimList || []).map(b => ({ ShortName: b.BirimKodu, Name: b.Aciklama }));
    }

    // ===========================
    // KALEM TABLO • SATIR ŞABLONU & HESAP
    // ===========================
    function unitOptionsHtml() {
        const list = w.__unitList?.length ? w.__unitList : [{ ShortName: DEFAULTS.UNIT_CODE, Name: DEFAULTS.UNIT_NAME }];
        return list.map(u => `<option value="${u.ShortName}">${u.Name} - ${u.ShortName}</option>`).join('');
    }
    function makeLineRow(idx) {
        return `
      <tr>
        <td class="text-center">${idx}</td>
        <td><input class="form-control ln-ad" placeholder="Mal/Hizmet Adı" value="GENEL ÜRÜN"></td>
        <td><input type="number" min="0" step="0.0001" class="form-control ln-qty" value="1"></td>
        <td><select class="form-control ln-unit">${unitOptionsHtml()}</select></td>
        <td><input type="number" step="0.01" class="form-control ln-price" value="1"></td>
        <td><input type="number" step="0.01" class="form-control ln-discp" value="0"></td>
        <td><input type="number" step="0.01" class="form-control ln-kdv" value="${DEFAULTS.VAT}"></td>
        <td class="hidden-xs"><input class="form-control ln-total" readonly value="1,20"></td>
        <td class="text-center"><button type="button" class="btn btn-xs btn-danger js-del-line"><i class="fa fa-trash"></i></button></td>
      </tr>`;
    }
    function renumberLines(linesSel) {
        $(`${linesSel} tbody tr`).each(function (i) { $(this).find('td:first').text(i + 1); });
    }
    function recalcTotals(linesSel) {
        let raw = 0, disc = 0, net = 0, kdv = 0, grand = 0;
        $(`${linesSel} tbody tr`).each(function () {
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
            const iskT = tutar * (discp / 100);
            const base = tutar - iskT;
            const kdvt = base * (rate / 100);
            const gross = base + kdvt;

            if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(gross));
            raw += qty * price;
            disc += iskT;
            net += base;
            kdv += kdvt;
            grand += gross;
        });
        setVal(SEL.tAra, fmt(raw) + ' TL');
        setVal(SEL.tIsk, fmt(disc) + ' TL');
        setVal(SEL.tMatrah20, fmt(net) + ' TL');
        setVal(SEL.tKdv20, fmt(kdv) + ' TL');
        setVal(SEL.tGenel, fmt(grand) + ' TL');
    }

    // ====== ÖZEL: #manuel_grid’e satır ekleme (klonlama) ======
    function addRow_manuelGrid() {
        const $tb = $('#manuel_grid tbody');
        const $last = $tb.find('tr:last');
        if (!$last.length) return;

        const $clone = $last.clone(true, true);

        // tüm input/select/textarea alanlarını temizle
        $clone.find('input,select,textarea').each(function () {
            const $el = $(this);
            const type = ($el.attr('type') || '').toLowerCase();
            if (type === 'checkbox' || type === 'radio') $el.prop('checked', false);
            else $el.val('');
        });

        // ln-* alanları için varsayılanlar
        $clone.find('.ln-qty').val('1');
        // .ln-unit select ise ilk option veya C62
        const $unit = $clone.find('.ln-unit');
        if ($unit.is('select')) {
            const firstOpt = $unit.find('option:first').val();
            $unit.val(firstOpt || 'C62');
        } else {
            $unit.val('C62');
        }
        $clone.find('.ln-price').val('1');
        $clone.find('.ln-discp').val('0');
        $clone.find('.ln-kdv').val(String(DEFAULTS.VAT));
        $clone.find('.ln-total').val('');

        // ilk kolon sıra no güncelle
        const idx = $tb.find('tr').length + 1;
        $clone.find('td:first').text(idx);

        // silme butonları çalışsın
        $clone.find('.js-del-line,.btnDel,.js-del-row').off('click._m').on('click._m', function () {
            $(this).closest('tr').remove();
            recalcTotals('#manuel_grid');
            saveDraft();
        });

        $tb.append($clone);
        recalcTotals('#manuel_grid');
        saveDraft();
    }

    // ===========================
    // LINES TABLOSUNU BAĞLA
    // ===========================
    function bindLineTable() {
        const linesSel = SEL.linesTable;
        const isManGrid = (linesSel === '#manuel_grid');

        function addLine() {
            if (isManGrid) {
                addRow_manuelGrid();
            } else {
                const idx = $(`${linesSel} tbody tr`).length + 1;
                $(`${linesSel} tbody`).append(makeLineRow(idx));
                recalcTotals(linesSel);
                saveDraft();
            }
        }

        // Bilinen buton id’leri
        $(SEL.btnAddLine).off('click.addline').on('click.addline', addLine);

        // Metne göre (Yeni Satır Ekle)
        $(d).off('click.addline_txt', SEL.btnAddByText).on('click.addline_txt', SEL.btnAddByText, function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t.indexOf('yeni') >= 0 && t.indexOf('satır') >= 0 && t.indexOf('ekle') >= 0) {
                addLine();
            }
        });

        // Sırf #manuel_grid caption’ı için özel bağlayıcı
        $(d).off('click.addline_cap_m', '#manuel_grid caption .btn, #manuel_grid caption button, #manuel_grid caption a')
            .on('click.addline_cap_m', '#manuel_grid caption .btn, #manuel_grid caption button, #manuel_grid caption a', function () {
                const t = ($(this).text() || '').trim().toLowerCase();
                if (t.indexOf('yeni') >= 0 && t.indexOf('ekle') >= 0) addRow_manuelGrid();
            });

        // Silme
        $(d).off('click.delline', '.js-del-line, .btnDel').on('click.delline', '.js-del-line, .btnDel', function () {
            $(this).closest('tr').remove();
            renumberLines(linesSel);
            recalcTotals(linesSel);
            saveDraft();
        });

        // Hesap tetikleyicileri
        $(d).off('input.recalc change.recalc', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad')
            .on('input.recalc change.recalc', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
                recalcTotals(linesSel);
                saveDraft();
            });

        $('#vatMode').off('change.recalc').on('change.recalc', function () {
            recalcTotals(linesSel);
            saveDraft();
        });

        // Boşsa bir satır
        if ($(`${linesSel} tbody tr`).length === 0) addLine();
    }

    // ===========================
    // BASİT GRIDLER (irsaliye & ek alanlar)
    // ===========================
    function simpleRowHtml(colsHtmlArr) {
        return `<tr>${colsHtmlArr.map(x => `<td>${x}</td>`).join('')}<td class="text-center"><button type="button" class="btn btn-danger btn-xs js-del-row">X</button></td></tr>`;
    }
    function bindSimpleGrid(addBtnSel, tableSel, inputs /* array of {type:'text'|'select', placeholder, list, getVal, getText} */) {
        if (!$(tableSel).length) return;

        const addRow = function () {
            const cols = (inputs || []).map(inp => {
                if (inp.type === 'select') {
                    const $sel = $('<select class="form-control">');
                    fillSelect($sel, (inp.list || []), inp.getVal || ((x) => x), inp.getText || ((x) => x), 'Seçiniz');
                    return $sel.prop('outerHTML');
                } else {
                    return `<input type="text" class="form-control" placeholder="${inp.placeholder || ''}">`;
                }
            });
            $(`${tableSel} tbody`).append(simpleRowHtml(cols));
        };

        // Bilinen buton id
        if ($(addBtnSel).length) $(addBtnSel).off(`click.add_${tableSel}`).on(`click.add_${tableSel}`, addRow);

        // Caption içindeki “Yeni Satır Ekle” metnine göre
        $(d).off(`click.addcap_${tableSel}`, `${tableSel} caption .btn, ${tableSel} caption button, ${tableSel} caption a`)
            .on(`click.addcap_${tableSel}`, `${tableSel} caption .btn, ${tableSel} caption button, ${tableSel} caption a`, function () {
                const txt = ($(this).text() || '').trim().toLowerCase();
                if (txt.indexOf('yeni') >= 0 && txt.indexOf('ekle') >= 0) addRow();
            });

        // Silme
        $(d).off(`click.del_${tableSel}`, `${tableSel} .js-del-row`).on(`click.del_${tableSel}`, `${tableSel} .js-del-row`, function () {
            $(this).closest('tr').remove();
        });
    }
    function bindAllSimpleGrids() {
        // İrsaliye (No - Tarih)
        bindSimpleGrid(SEL.btnDespatchAdd, SEL.despatchTable, [
            { type: 'text', placeholder: 'İrsaliye No' },
            { type: 'text', placeholder: 'İrsaliye Tarihi' }
        ]);

        // Satıcı Ek Alan (Tanıtıcı Kod/Değer)
        bindSimpleGrid(SEL.btnSellerExtra, SEL.sellerExtraTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);

        // Satıcı Agent Ek Alan
        bindSimpleGrid(SEL.btnSellerAgent, SEL.sellerAgentTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);

        // Alıcı Ek Alan
        bindSimpleGrid(SEL.btnBuyerExtra, SEL.buyerExtraTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);

        // Ek güvence: caption’da “Yeni Satır Ekle” yakalayıcı
        $(d).off('click.add_caption_all', 'table caption .btn, table caption button, table caption a')
            .on('click.add_caption_all', 'table caption .btn, table caption button, table caption a', function () {
                const txt = ($(this).text() || '').trim().toLowerCase();
                if (txt.indexOf('yeni') === -1 || txt.indexOf('ekle') === -1) return;
                const $table = $(this).closest('table');
                const id = ($table.attr('id') || '').toLowerCase();
                if (id === 'irsaliye_grid') $(SEL.btnDespatchAdd).trigger('click');
                if (id === 'saticiekalan_grid') $(SEL.btnSellerExtra).trigger('click');
                if (id === 'saticiagentekalan_grid') $(SEL.btnSellerAgent).trigger('click');
                if (id === 'aliciekalan_grid') $(SEL.btnBuyerExtra).trigger('click');
            });
    }

    // ===========================
    // TASLAK • KAYDET/YÜKLE
    // ===========================
    function saveDraft(dto) { try { localStorage.setItem(DEFAULTS.APP_KEY, JSON.stringify(dto || collectInvoice())); } catch { } }
    function loadDraft() {
        try {
            const raw = localStorage.getItem(DEFAULTS.APP_KEY);
            if (!raw) return;
            applyDraft(JSON.parse(raw));
        } catch { }
    }
    function clearDraft() { try { localStorage.removeItem(DEFAULTS.APP_KEY); } catch { } }

    // ===========================
    // VERGİ HESABI (oran bazlı özet)
    // ===========================
    function computeTaxes(linesSel) {
        const bag = {}; // rate -> {base, amount}
        $(`${linesSel} tbody tr`).each(function () {
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
            const iskT = tutar * (discp / 100);
            const base = tutar - iskT;
            const kdvt = base * (rate / 100);

            if (!bag[rate]) bag[rate] = { base: 0, amount: 0 };
            bag[rate].base += base;
            bag[rate].amount += kdvt;
        });
        return bag;
    }

    // ===========================
    // DTO OLUŞTURMA
    // ===========================
    function collectInvoice() {
        const now = new Date().toISOString();
        const linesSel = SEL.linesTable;
        const currency = ($(SEL.currency).val() || DEFAULTS.CURRENCY).toUpperCase();

        const dto = {
            entity: 'EInvoice',
            invoiceNo: ($(SEL.prefix).val() || 'INV') + '-' + Date.now(),
            invoiceDate: now,
            currency,
            total: dec($(SEL.tGenel).val() || $(SEL.tGenel).text() || '0'),
            createdAt: now, updatedAt: now,

            customer: {
                name: $(SEL.custName).val() || '',
                surname: $(SEL.custSurname).val() || '',
                email: $(SEL.custMail).val() || '',
                taxNo: $(SEL.custTaxNo).val() || '',
                taxOffice: $(SEL.custTaxOffice).val() || '',
                createdAt: now, updatedAt: now,
                customersGroups: [], addresses: [], invoices: []
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
        $(`${linesSel} tbody tr`).each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            const rate = dec($r.find('.ln-kdv').val()) || 0;

            const unitShort = ($r.find('.ln-unit').val() || DEFAULTS.UNIT_CODE).toUpperCase();
            const unitName = (unitShort === 'C62' ? DEFAULTS.UNIT_NAME : unitShort);

            // Satır toplamı: ln-total varsa onu, yoksa hesapla (DAHİL/HARİÇ desteği)
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
                createdAt: now, updatedAt: now,
                item: {
                    name: $r.find('.ln-ad').val() || 'GENEL ÜRÜN',
                    code: 'ITEM-' + Math.floor(Math.random() * 100000),
                    currency,
                    createdAt: now, updatedAt: now,
                    unit: { shortName: unitShort, name: unitName, createdAt: now, updatedAt: now }
                }
            });
        });

        // Vergiler
        const taxes = computeTaxes(linesSel);
        Object.keys(taxes).forEach(k => {
            dto.invoicesTaxes.push({ name: 'KDV', rate: Number(k), amount: taxes[k].amount, createdAt: now, updatedAt: now });
        });

        // Genel iskonto
        let totalDisc = 0;
        $(`${linesSel} tbody tr`).each(function () {
            const $r = $(this);
            const qty = dec($r.find('.ln-qty').val());
            const price = dec($r.find('.ln-price').val());
            const discp = dec($r.find('.ln-discp').val());
            let tutar = qty * price;
            if (getVatMode() === VAT_MODE.INCL) {
                const unitNet = price / (1 + (dec($r.find('.ln-kdv').val()) || 0) / 100);
                tutar = qty * unitNet;
            }
            totalDisc += tutar * (discp / 100);
        });
        dto.invoicesDiscounts.push({ name: 'Toplam İskonto', desc: 'Otomatik', base: 'Ara Toplam', rate: 0, amount: totalDisc, createdAt: now, updatedAt: now });

        // SGK
        if ($(SEL.sgkType).val()) {
            dto.sgkRecords.push({
                type: 'SGK',
                code: 'SGK001',
                name: $(SEL.sgkType).val(),
                no: $(SEL.sgkDosya).val() || '0',
                startDate: ($(SEL.sgkStart).val() ? new Date($(SEL.sgkStart).val()).toISOString() : now),
                endDate: ($(SEL.sgkEnd).val() ? new Date($(SEL.sgkEnd).val()).toISOString() : now),
                createdAt: now, updatedAt: now
            });
        }

        // Ödeme
        dto.invoicesPayments.push({
            createdAt: now, updatedAt: now,
            payment: {
                amount: dto.total, currency, date: now, note: $(SEL.payNote).val() || '',
                createdAt: now, updatedAt: now,
                paymentType: { name: $(SEL.payMeans).find('option:selected').text() || '', createdAt: now, updatedAt: now },
                paymentAccount: { name: $(SEL.payAccount).val() || '', createdAt: now, updatedAt: now },
                bank: { name: $(SEL.bankName).val() || 'Banka', createdAt: now, updatedAt: now }
            }
        });

        // Servis sağlayıcı
        dto.servicesProviders.push({ no: 'SP-001', systemUser: 'UI', createdAt: now, updatedAt: now });

        return dto;
    }

    // ===========================
    // TASLAĞI UYGULA
    // ===========================
    function applyDraft(dto) {
        try {
            setVal(SEL.custName, dto.customer?.name || '');
            setVal(SEL.custSurname, dto.customer?.surname || '');
            setVal(SEL.custMail, dto.customer?.email || '');
            setVal(SEL.custTaxNo, dto.customer?.taxNo || '');
            setVal(SEL.custTaxOffice, dto.customer?.taxOffice || '');
            setVal(SEL.currency, dto.currency || DEFAULTS.CURRENCY);

            const linesSel = SEL.linesTable;
            $(`${linesSel} tbody`).empty();
            const items = dto.invoicesItems || [];
            if (items.length === 0) {
                $(`${linesSel} tbody`).append(makeLineRow(1));
            } else {
                items.forEach((x, i) => {
                    $(`${linesSel} tbody`).append(makeLineRow(i + 1));
                    const $r = $(`${linesSel} tbody tr`).last();
                    $r.find('.ln-ad').val(x.item?.name || '');
                    $r.find('.ln-qty').val(x.quantity ?? 1);
                    $r.find('.ln-unit').val(x.item?.unit?.shortName || DEFAULTS.UNIT_CODE);
                    $r.find('.ln-price').val(x.price ?? 1);
                    $r.find('.ln-discp').val(0);
                    $r.find('.ln-kdv').val((dto.invoicesTaxes?.[0]?.rate) ?? DEFAULTS.VAT);
                    $r.find('.ln-total').val(fmt(x.total ?? 0));
                });
            }
            recalcTotals(linesSel);
        } catch { clearDraft(); }
    }

    // ===========================
    // API KÖPRÜSÜ (InvoiceApi veya fallback)
    // ===========================
    function getInvoiceApi() {
        if (w.InvoiceApi && typeof w.InvoiceApi.create === 'function') return w.InvoiceApi;
        return {
            create: (dto) => $.ajax({ url: '/api/v1/invoice', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' }),
            send: (dto) => $.ajax({ url: '/api/v1/invoice/send', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' })
        };
    }

    async function doSave() {
        console.log('💾 Fatura kaydediliyor...');
        const dto = collectInvoice();
        if (!(dto.invoicesItems || []).length) { alert('En az bir satır ekleyin.'); return; }
        try {
            console.log(dto);
            const res = await InvoiceApi.create(dto);
            (w.toastr ? toastr.success('Fatura başarıyla kaydedildi.') : alert('Fatura başarıyla kaydedildi.'));
            log('✅ API Yanıtı:', res);
            clearDraft();
        } catch (e) {
            const msg = e?.responseText || e?.message || ('' + e);
            err('❌ Hata:', e);
            (w.toastr ? toastr.error('Fatura kaydedilemedi: ' + msg) : alert('Fatura kaydedilemedi: ' + msg));
        }
    }
    async function doSend() {
        const dto = collectInvoice();
        try { await getInvoiceApi().send(dto); alert('GİB’e gönderildi.'); clearDraft(); }
        catch (e) { alert('Gönderim hatası: ' + (e?.responseText || e?.message || e)); }
    }

    // ===========================
    // ÖNİZLEME / İNDİRME
    // ===========================
    function openPreview(dto) {
        if ($('#modalPreview').length && $('#previewContent').length) {
            const rows = (dto.invoicesItems || []).map((x, i) => `<tr><td>${i + 1}</td><td>${x.item?.name || ''}</td><td class="text-right">${x.quantity}</td><td class="text-right">${x.item?.unit?.shortName || ''}</td><td class="text-right">${fmt(x.price)}</td><td class="text-right">${fmt(x.total)}</td></tr>`).join('');
            const html = `<div class="table-responsive"><table class="table table-sm table-striped"><thead><tr><th>#</th><th>Ürün</th><th>Miktar</th><th>Birim</th><th>Birim Fiyat</th><th>Tutar</th></tr></thead><tbody>${rows}</tbody><tfoot><tr><th colspan="5" class="text-right">Genel Toplam</th><th class="text-right">${fmt(dto.total)}</th></tr></tfoot></table></div>`;
            $('#previewContent').html(html);
            $('#modalPreview').modal('show');
            return true;
        }
        if ($('#modal-onizleme').length && $('#onizle-iframe').length) {
            let html = '<html><head><meta charset="utf-8"><title>Önizleme</title>';
            html += '<style>body{font-family:Arial;padding:14px}table{width:100%;border-collapse:collapse}td,th{border:1px solid #ccc;padding:6px;font-size:12px}</style>';
            html += '</head><body>';
            html += '<h3>e-Fatura Önizleme</h3>';
            const linesSel = SEL.linesTable;
            html += (document.querySelector(linesSel)?.outerHTML || '');
            html += `<p><b>Genel Toplam:</b> ${fmt(dto.total)}</p>`;
            html += '</body></html>';
            const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
            $('#onizle-iframe').attr('src', url);
            $('#modal-onizleme').modal('show');
            return true;
        }
        return false;
    }
    async function doPreview() {
        const dto = collectInvoice();
        if (!dto || !dto.invoicesItems || !dto.invoicesItems.length) { alert('En az bir satır ekleyin.'); return; }
        if (!openPreview(dto)) {
            let html = '<html><head><meta charset="utf-8"><title>Önizleme</title></head><body>';
            html += '<h3>e-Fatura Önizleme</h3>';
            const linesSel = SEL.linesTable;
            html += (document.querySelector(linesSel)?.outerHTML || '');
            html += `<p><b>Genel Toplam:</b> ${fmt(dto.total)}</p>`;
            html += '</body></html>';
            const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
            w.open(url, '_blank');
        }
    }

    async function ensureCreated(dto) {
        const res = await getInvoiceApi().create(dto);
        return res?.id || res?.data?.id || res?.guid || null;
    }
    async function doDownloadPdf() {
        const dto = collectInvoice();
        if (!dto || !dto.invoicesItems || !dto.invoicesItems.length) { alert('En az bir satır ekleyin.'); return; }
        try {
            $('#modalBusy').modal && $('#modalBusy').modal('show');
            const id = await ensureCreated(dto);
            if (!id) throw new Error('Fatura oluşturulamadı.');
            w.open(`/api/v1/invoice/pdf/${id}`, '_blank');
        } catch (e) {
            alert('PDF indirilemedi: ' + (e?.responseText || e?.message || e));
        } finally {
            $('#modalBusy').modal && $('#modalBusy').modal('hide');
        }
    }
    async function doDownloadXml() {
        const dto = collectInvoice();
        if (!dto || !dto.invoicesItems || !dto.invoicesItems.length) { alert('En az bir satır ekleyin.'); return; }
        try {
            $('#modalBusy').modal && $('#modalBusy').modal('show');
            const id = await ensureCreated(dto);
            if (!id) throw new Error('Fatura oluşturulamadı.');
            w.open(`/api/v1/invoice/xml/${id}`, '_blank');
        } catch (e) {
            alert('XML indirilemedi: ' + (e?.responseText || e?.message || e));
        } finally {
            $('#modalBusy').modal && $('#modalBusy').modal('hide');
        }
    }

    // ===========================
    // ACTION BINDINGS
    // ===========================
    function bindActions() {
        console.log("🔗 Buton eylemleri bağlanıyor...");
        $(SEL.btnSave).off('click.save').on('click.save', doSave);
        $(SEL.btnSend).off('click.send').on('click.send', doSend);

        $(SEL.btnPreview).off('click.prev').on('click.prev', doPreview);
        $(SEL.btnPdf).off('click.pdf').on('click.pdf', doDownloadPdf);
        $(SEL.btnXml).off('click.xml').on('click.xml', doDownloadXml);

        // Metne göre PDF/XML butonlarına da bağlan (opsiyonel güvence)
        $(d).off('click.pdf_text', 'button,a').on('click.pdf_text', 'button,a', function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t === 'pdf indir' || t.indexOf('pdf') >= 0) doDownloadPdf();
        });
        $(d).off('click.xml_text', 'button,a').on('click.xml_text', 'button,a', function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t === 'xml indir' || t.indexOf('xml') >= 0) doDownloadXml();
        });

        // Otomatik taslak kaydı (10sn)
        setInterval(() => saveDraft(), 10000);
    }

    // ===========================
    // INIT
    // ===========================
    $(function () {
        console.log('🧾 Invoice.js (final+patch) yüklendi');

        // lines tablosunu DOM yüklendikten sonra belirle — manuel_grid > tblLines > lines
        SEL.linesTable = (document.getElementById('manuel_grid') ? '#manuel_grid'
            : (document.getElementById('tblLines') ? '#tblLines'
                : (document.getElementById('lines') ? '#lines' : '#tblLines')));

        // “Nox Yazılım” vb. alt yazıları gizle
        $('.help-block, small').filter(function () {
            const t = ($(this).text() || '').toLowerCase();
            return t.indexOf('nox') >= 0 && t.indexOf('yazılım') >= 0;
        }).hide();

        safeInitPickers();
        loadLookups();
        bindLineTable();
        bindAllSimpleGrids();
        bindActions();
        loadDraft();

        // “Yeni Satır Ekle” metnini taşıyan butonlara güvence için sınıf ver
        $('button, a').filter(function () { return ($(this).text() || '').trim().toLowerCase().indexOf('yeni satır ekle') >= 0; })
            .addClass('btn-new-line');

        // Dışarı debug
        w.EinvoiceUI = { collectInvoice, recalcTotals, saveDraft, applyDraft };
    });

    /* ==== BEGIN: EInvoice patch (non-destructive, append-only) ==== */
    (function ($, w, d) {
        'use strict';

        if (w.__EINV_PATCH_APPLIED__) return; // iki kez eklenmesin
        w.__EINV_PATCH_APPLIED__ = true;

        // 1) Başlık–gövde hizası (TH/TD)
        if (typeof w.fixLinesHeaderAlignment !== 'function') {
            w.fixLinesHeaderAlignment = function fixLinesHeaderAlignment() {
                try {
                    var $tbl = $('#tblLines').length ? $('#tblLines')
                        : ($('#lines').length ? $('#lines')
                            : ($('#manuel_grid').length ? $('#manuel_grid') : $()));
                    if (!$tbl.length) return;

                    // thead th ve ilk görünür satırın td’leri eşitlenir
                    var $head = $tbl.find('thead th');
                    var $row = $tbl.find('tbody tr:visible:first');
                    if (!$head.length || !$row.length) return;

                    var $tds = $row.children('td');
                    if ($tds.length !== $head.length) return;

                    $tds.each(function (i) {
                        var w = $(this).outerWidth();
                        $($head[i]).css('width', w);
                    });
                } catch (e) { /* yok say */ }
            };
        }

        // 2) Satır işlem butonları (+ / ✎ / 🗑) – şablona dokunmadan, çalışma anında ekle
        function ensureRowActionButtons($tbl) {
            try {
                $tbl.find('tbody tr').each(function () {
                    var $last = $(this).children('td').last();
                    if (!$last.length) return;

                    var hasGroup = $last.find('.einv-btn-group').length > 0;
                    if (!hasGroup) {
                        // Sil butonu zaten varsa kalsın; biz sadece + ve ✎ ekleyelim
                        var $del = $last.find('.js-del-line, .btnDel').first();
                        var $grp = $('<div class="btn-group btn-group-xs einv-btn-group" role="group" style="margin-left:4px;"></div>');

                        // + butonu
                        $('<button type="button" class="btn btn-success js-line-add" title="Altına Satır Ekle"><i class="fa fa-plus"></i></button>')
                            .appendTo($grp);

                        // ✎ butonu
                        $('<button type="button" class="btn btn-warning js-line-edit" title="Düzenle"><i class="fa fa-pencil"></i></button>')
                            .appendTo($grp);

                        // Grup yerleşimi: varsa sil butonunun SAĞINA ekle; yoksa tek başına ekle
                        if ($del.length) {
                            $del.after($grp);
                        } else {
                            // hiç sil butonu yoksa komple grubu ekle
                            $last.append($grp);
                            // ve yoksa bir de sil butonu ekleyelim
                            $(
                                '<button type="button" class="btn btn-danger js-del-line" title="Sil" style="margin-left:4px;"><i class="fa fa-trash"></i></button>'
                            ).appendTo($grp);
                        }
                    }
                });
            } catch (e) { }
        }

        // 3) Toplam hesap – var olan recalc/recalcTotals neyse onu kullan
        function triggerTotals() {
            try {
                if (typeof w.recalcTotals === 'function') {
                    // bazı projelerde linesSel gerekir; bazısında gerekmez
                    try { w.recalcTotals((w.SEL && w.SEL.linesTable) || '#tblLines'); }
                    catch { w.recalcTotals(); }
                } else if (typeof w.recalc === 'function') {
                    w.recalc();
                }
            } catch (e) { }
        }

        // 4) Satır numarası – varsa mevcut fonksiyonu çağır
        function triggerRenumber() {
            try {
                if (typeof w.renumberLines === 'function') {
                    w.renumberLines((w.SEL && w.SEL.linesTable) || '#tblLines');
                } else if (typeof w.renumber === 'function') {
                    w.renumber();
                }
            } catch (e) { }
        }

        // 5) + / ✎ / 🗑 olayları – delege
        $(d)
            // + altına satır ekle
            .off('click.einv.add', '.js-line-add')
            .on('click.einv.add', '.js-line-add', function () {
                var $tr = $(this).closest('tr');
                var $tbl = $tr.closest('table');
                var idx = $tbl.find('tbody tr').length + 1;

                // Tercihen mevcut rowTemplate/makeLineRow fonksiyonlarını kullan, yoksa satırı klonla
                try {
                    if (typeof w.rowTemplate === 'function') {
                        $tr.after(w.rowTemplate(idx));
                    } else if (typeof w.makeLineRow === 'function') {
                        $tr.after(w.makeLineRow(idx));
                    } else {
                        var $clone = $tr.clone(true, true);
                        // değerleri sıfırla/temizle
                        $clone.find('.ln-qty').val('1');
                        $clone.find('.ln-price').val('1');
                        $clone.find('.ln-discp').val('0');
                        $clone.find('.ln-kdv').val($clone.find('.ln-kdv').val() || '20');
                        $clone.find('.ln-total').val('');
                        $tr.after($clone);
                    }
                } catch { /* fallback klon yukarıda */ }

                triggerRenumber();
                triggerTotals();
                w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                // buton grubu yeni satıra da gelsin
                ensureRowActionButtons($tbl);
            })

            // ✎ düzenle toggle
            .off('click.einv.edit', '.js-line-edit')
            .on('click.einv.edit', '.js-line-edit', function () {
                var $tr = $(this).closest('tr');
                var $eds = $tr.find('.ln-ad,.ln-qty,.ln-unit,.ln-price,.ln-discp,.ln-kdv');
                var disabled = $eds.prop('disabled');
                $eds.prop('disabled', !disabled);
                if (!disabled) { $eds.first().focus(); }
            })

            // 🗑 sil – mevcut handler varsa da çalışır, sonunda hizayı düzelt
            .off('click.einv.del.after', '.js-del-line, .btnDel')
            .on('click.einv.del.after', '.js-del-line, .btnDel', function () {
                setTimeout(function () {
                    triggerRenumber();
                    triggerTotals();
                    w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                }, 0);
            })

            // Hesap tetikleyicileri – alan değişimleri
            .off('input.einv change.einv', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad')
            .on('input.einv change.einv', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
                triggerTotals();
            });

        // 6) Hazır olunca ilk hizalama ve buton ekleme
        $(function () {
            var $tbl = $('#tblLines').length ? $('#tblLines')
                : ($('#lines').length ? $('#lines')
                    : ($('#manuel_grid').length ? $('#manuel_grid') : $()));
            if ($tbl.length) {
                ensureRowActionButtons($tbl);
                setTimeout(function () {
                    w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                }, 100);
                $(w).on('resize.einv', function () {
                    w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                });
            }
        });

    })(jQuery, window, document);
    /* ==== END: EInvoice patch ==== */


})(jQuery, window, document);




