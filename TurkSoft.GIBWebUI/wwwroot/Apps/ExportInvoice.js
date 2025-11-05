// wwwroot/apps/invoice.ihracat.js
// jQuery orchestrator for e-İhracat Create page (Angular-free)
// Works against #manuel_grid, irsaliye_grid and buttons in İhracat.txt.
// (c) you
import { InvoiceApi } from '../Entites/index.js';
(function ($, w, d) {
    'use strict';

    const SEL = {
        lines: '#manuel_grid',
        despatch: '#irsaliye_grid',
        vatMode: '[name="kdvstatu"]',
        btnDraft: '#btn_taslak, [data-js="save-draft"]',
        btnSend: '#btn_gonder, [data-js="send"]',
        btnPreview: '#btnPreview, #btnOnizleme, [data-js="preview"]',
        btnDlPdf: '[data-js="dl-pdf"]',
        btnDlXml: '[data-js="dl-xml"]',
        modalPreview: '#modal-onizleme',
        iframePreview: '#onizle-iframe'
    };

    const DEFAULTS = {
        UNIT: 'C62',
        VAT: 20,
        CURRENCY: 'TRY',
        APP_KEY: 'einvoice_export_draft_v1'
    };

    const VAT_MODE = { EXCL: 'HARIC', INCL: 'DAHIL' };

    const log = (...a) => console.log(...a);
    const err = (...a) => console.error(...a);

    function dec(v) {
        v = ('' + (v ?? '')).replace(' TL', '').trim().replace(/\./g, '').replace(',', '.');
        const n = parseFloat(v);
        return Number.isFinite(n) ? n : 0;
    }
    function fmt(n) {
        n = (Math.round((n ?? 0) * 100) / 100).toFixed(2);
        const p = n.split('.');
        p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        return p.join(',');
    }
    function getVatMode() {
        const val = ($(SEL.vatMode).val() || '').toString().toLowerCase();
        return (val === 'true' || val === '1' || val.indexOf('dahil') >= 0) ? VAT_MODE.INCL : VAT_MODE.EXCL;
    }

    function $rows() { return $(`${SEL.lines} tbody tr`); }
    function readRow($r) {
        const get = name => $r.find(`[name="${name}"]`);
        const qty = dec(get('Quantity_Amount').val());
        const unit = (get('Quantity_Unit_User').val() || DEFAULTS.UNIT).toUpperCase();
        const price = dec(get('Price_Amount').val());
        const discp = dec(get('Allowance_Percent').val());
        const rate = dec(get('Tax_Perc0015').val()) || 0;
        return {
            qty, unit, price, discp, rate,
            $qty: get('Quantity_Amount'), $unit: get('Quantity_Unit_User'), $price: get('Price_Amount'),
            $discp: get('Allowance_Percent'), $discAmt: get('Allowance_Amount'),
            $rate: get('Tax_Perc0015'), $vatAmt: get('Tax_Amnt0015'), $lineTot: get('Price_Total'),
            $name: get('MalAdi')
        };
    }

    function calcRow(r) {
        let tutar = r.qty * r.price;
        if (getVatMode() === VAT_MODE.INCL) {
            const unitNet = r.price / (1 + r.rate / 100);
            tutar = r.qty * unitNet;
        }
        const isk = tutar * (r.discp / 100);
        const base = tutar - isk;
        const kdvt = base * (r.rate / 100);
        const gross = base + kdvt;
        return { isk, base, kdvt, gross };
    }

    function recalc() {
        let raw = 0, isk = 0, net = 0, kdv = 0, grand = 0;
        $rows().each(function () {
            const r = readRow($(this));
            const c = calcRow(r);
            if (r.$discAmt.length) r.$discAmt.val(fmt(c.isk));
            if (r.$vatAmt.length) r.$vatAmt.val(fmt(c.kdvt));
            if (r.$lineTot.length) r.$lineTot.val(fmt(c.gross));
            raw += r.qty * r.price;
            isk += c.isk; net += c.base; kdv += c.kdvt; grand += c.gross;
        });
        $('#tAra').text(fmt(raw) + ' TL'); $('#tIsk').text(fmt(isk) + ' TL');
        $('#tMatrah20').text(fmt(net) + ' TL'); $('#tKdv20').text(fmt(kdv) + ' TL');
        $('#tGenel').text(fmt(grand) + ' TL');
    }

    function addRow() {
        const $tb = $(`${SEL.lines} tbody`);
        const $last = $tb.find('tr:last');
        if (!$last.length) return;
        const $clone = $last.clone(true, true);
        $clone.find('input,select,textarea').each(function () {
            const $el = $(this);
            const type = ($el.attr('type') || '').toLowerCase();
            if (type === 'checkbox' || type === 'radio') $el.prop('checked', false);
            else $el.val('');
        });
        // varsayılanlar
        $clone.find('[name="Quantity_Amount"]').val('1');
        $clone.find('[name="Quantity_Unit_User"]').val(DEFAULTS.UNIT);
        $clone.find('[name="Price_Amount"]').val('1');
        $clone.find('[name="Allowance_Percent"]').val('0');
        $clone.find('[name="Tax_Perc0015"]').val(String(DEFAULTS.VAT));
        $tb.append($clone);
        recalc(); saveDraft();
    }

    function bindRowDeletes() {
        $(d).off('click.rowdel', `${SEL.lines} .btn-danger, ${SEL.lines} .js-del-row`).on('click.rowdel', `${SEL.lines} .btn-danger, ${SEL.lines} .js-del-row`, function () {
            $(this).closest('tr').remove();
            recalc(); saveDraft();
        });
    }

    function computeTaxes() {
        const bag = {};
        $rows().each(function () {
            const r = readRow($(this));
            const c = calcRow(r);
            const k = String(r.rate);
            if (!bag[k]) bag[k] = { base: 0, amount: 0 };
            bag[k].base += c.base;
            bag[k].amount += c.kdvt;
        });
        return bag;
    }

    function collectDto() {
        const now = new Date().toISOString();
        const currency = ($('[name="DocumentCurrencyCode"]').val() || DEFAULTS.CURRENCY).toUpperCase();
        const dto = {
            entity: 'EInvoice',
            invoiceNo: 'EXP-' + Date.now(),
            invoiceDate: now,
            currency,
            total: dec($('#tGenel').text() || '0'),
            createdAt: now, updatedAt: now,

            customer: {
                name: $('#txtPartyName').val() || '',
                surname: $('#txtPerson_FamilyName').val() || '',
                email: $('#txtElectronicMail').val() || '',
                taxNo: $('#txtIdentificationID').val() || '',
                taxOffice: $('#txtTaxSchemeName').val() || '',
                createdAt: now, updatedAt: now, customersGroups: [], addresses: [], invoices: []
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

        $rows().each(function () {
            const r = readRow($(this));
            const c = calcRow(r);
            dto.invoicesItems.push({
                quantity: r.qty,
                price: r.price,
                total: c.gross,
                createdAt: now, updatedAt: now,
                item: {
                    name: (r.$name.val() || 'GENEL ÜRÜN'),
                    code: 'ITEM-' + Math.floor(Math.random() * 100000),
                    currency,
                    createdAt: now, updatedAt: now,
                    unit: { shortName: r.unit, name: r.unit, createdAt: now, updatedAt: now }
                }
            });
        });

        const taxes = computeTaxes();
        Object.keys(taxes).forEach(k => dto.invoicesTaxes.push({ name: 'KDV', rate: Number(k), amount: taxes[k].amount, createdAt: now, updatedAt: now }));

        let totalDisc = 0;
        $rows().each(function () {
            const r = readRow($(this));
            let tutar = r.qty * r.price;
            if (getVatMode() === VAT_MODE.INCL) tutar = r.qty * (r.price / (1 + r.rate / 100));
            totalDisc += tutar * (r.discp / 100);
        });
        dto.invoicesDiscounts.push({ name: 'Toplam İskonto', desc: 'Otomatik', base: 'Ara Toplam', rate: 0, amount: totalDisc, createdAt: now, updatedAt: now });

        // >>> DÜZELTME: Banka artık PaymentAccount altında
        dto.invoicesPayments.push({
            createdAt: now, updatedAt: now,
            payment: {
                amount: dto.total, currency, date: now, note: '',
                createdAt: now, updatedAt: now,
                paymentType: { name: 'NAKIT', createdAt: now, updatedAt: now },
                paymentAccount: {
                    name: 'KASA',
                    createdAt: now, updatedAt: now,
                    bank: { name: 'Banka', createdAt: now, updatedAt: now } // doğru yer
                }
            }
        });

        dto.servicesProviders.push({ no: 'SP-EXP', systemUser: 'UI', createdAt: now, updatedAt: now });

        return dto;
    }

    function openPreview(dto) {
        if ($(SEL.modalPreview).length && $(SEL.iframePreview).length) {
            let html = '<html><head><meta charset="utf-8"><title>Önizleme</title>';
            html += '<style>body{font-family:Arial;padding:14px}table{width:100%;border-collapse:collapse}td,th{border:1px solid #ccc;padding:6px;font-size:12px}</style>';
            html += '</head><body>';
            html += '<h3>e-İhracat Önizleme</h3>';
            html += (document.querySelector(SEL.lines)?.outerHTML || '');
            html += `<p><b>Genel Toplam:</b> ${$('#tGenel').text() || ''}</p>`;
            html += '</body></html>';
            const url = URL.createObjectURL(new Blob([html], { type: 'text/html' }));
            $(SEL.iframePreview).attr('src', url);
            $(SEL.modalPreview).modal && $(SEL.modalPreview).modal('show');
            return true;
        }
        return false;
    }

    function getInvoiceApi() {
        if (w.InvoiceApi && typeof w.InvoiceApi.create === 'function') return w.InvoiceApi;
        if (InvoiceApi && typeof InvoiceApi.create === 'function') return InvoiceApi;
        // güvenli fallback
        return {
            create: dto => $.ajax({ url: '/api/v1/invoice', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' }),
            send: dto => $.ajax({ url: '/api/v1/invoice/send', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' })
        };
    }

    async function doSave() {
        const dto = collectDto();
        if (!(dto.invoicesItems || []).length) { alert('En az bir satır ekleyin.'); return; }
        try {
            console.log(dto);
            const res = await InvoiceApi.create(dto);
            (w.toastr ? toastr.success('Fatura taslağı kaydedildi.') : alert('Fatura taslağı kaydedildi.'));
            log('✅ API Yanıtı:', res);
            clearDraft();
        } catch (e) {
            const msg = e?.responseText || e?.message || ('' + e);
            (w.toastr ? toastr.error('Kaydedilemedi: ' + msg) : alert('Kaydedilemedi: ' + msg));
        }
    }
    async function doSend() {
        const dto = collectDto();
        try {
            await getInvoiceApi().send(dto); // >>> düzeltilmiş (fallback’li)
            alert('GİB’e gönderildi.');
            clearDraft();
        } catch (e) { alert('Gönderim hatası: ' + (e?.responseText || e?.message || e)); }
    }

    function saveDraft() { try { localStorage.setItem(DEFAULTS.APP_KEY, JSON.stringify(collectDto())); } catch { } }
    function loadDraft() {
        try {
            const raw = localStorage.getItem(DEFAULTS.APP_KEY);
            if (!raw) return;
            const dto = JSON.parse(raw);
            const $tb = $(`${SEL.lines} tbody`);
            $tb.find('tr').slice(1).remove();
            const items = dto.invoicesItems || [];
            items.forEach((x, i) => {
                if (i > 0) addRow();
                const $r = $tb.find('tr').eq(i);
                $r.find('[name="MalAdi"]').val(x.item?.name || '');
                $r.find('[name="Quantity_Amount"]').val(x.quantity ?? 1);
                $r.find('[name="Quantity_Unit_User"]').val(x.item?.unit?.shortName || DEFAULTS.UNIT);
                $r.find('[name="Price_Amount"]').val(x.price ?? 1);
                $r.find('[name="Allowance_Percent"]').val('0');
                $r.find('[name="Tax_Perc0015"]').val((dto.invoicesTaxes?.[0]?.rate) ?? DEFAULTS.VAT);
            });
            recalc();
        } catch { }
    }
    function clearDraft() { try { localStorage.removeItem(DEFAULTS.APP_KEY); } catch { } }

    function populateSelects() {
        // Birim
        if (w.birimList && $('[name="Quantity_Unit_User"]').length) {
            const $s = $('[name="Quantity_Unit_User"]');
            if ($s.children('option').length <= 1) {
                w.birimList.forEach(x => $s.append(`<option value="${x.BirimKodu}">${x.Aciklama}</option>`));
            }
        }
        // Para Birimi
        if (w.parabirimList && $('[name="DocumentCurrencyCode"]').length) {
            const $c = $('[name="DocumentCurrencyCode"]');
            if ($c.children('option').length <= 1) {
                w.parabirimList.forEach(x => $c.append(`<option value="${x.Kodu}">${x.Aciklama}</option>`));
            }
        }
        // Ülke
        if (w.ulkeList && $('[name="selulke"]').length) {
            const $u = $('[name="selulke"]');
            if ($u.children('option').length <= 1) {
                w.ulkeList.forEach(x => $u.append(`<option value="${x.UlkeKodu}">${x.UlkeAdi}</option>`));
            }
        }
        // İl
        if (w.ilList && $('[name="selIl"]').length) {
            const $il = $('[name="selIl"]');
            if ($il.children('option').length <= 1) {
                w.ilList.forEach(x => $il.append(`<option value="${x.IlAdi}">${x.IlAdi}</option>`));
            }
        }
    }

    function bindUi() {
        $(d).off('click.addline', 'button, a').on('click.addline', 'button, a', function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t.indexOf('yeni') >= 0 && t.indexOf('satır') >= 0 && t.indexOf('ekle') >= 0) { addRow(); }
        });

        $(d).off('input.recalc change.recalc', `${SEL.lines} [name="Quantity_Amount"], ${SEL.lines} [name="Price_Amount"], ${SEL.lines} [name="Allowance_Percent"], ${SEL.lines} [name="Tax_Perc0015"], ${SEL.vatMode}`)
            .on('input.recalc change.recalc', `${SEL.lines} [name="Quantity_Amount"], ${SEL.lines} [name="Price_Amount"], ${SEL.lines} [name="Allowance_Percent"], ${SEL.lines} [name="Tax_Perc0015"], ${SEL.vatMode}`, recalc);

        bindRowDeletes();

        $(SEL.btnDraft).off('click.save').on('click.save', doSave);
        $(SEL.btnSend).off('click.send').on('click.send', doSend);

        $(d).off('click.preview').on('click.preview', SEL.btnPreview, function () {
            openPreview(collectDto());
        });

        $(d).off('click.dlpdf').on('click.dlpdf', SEL.btnDlPdf, function (e) { e.preventDefault(); alert('PDF indirme entegrasyonu eklenecek.'); });
        $(d).off('click.dlxml').on('click.dlxml', SEL.btnDlXml, function (e) { e.preventDefault(); alert('XML indirme entegrasyonu eklenecek.'); });
    }

    $(function () {
        log('🧾 e-İhracat sayfası hazır.');
        populateSelects();
        bindUi();
        recalc();
        loadDraft();
        const $tb = $(`${SEL.lines} tbody`);
        if ($tb.find('tr').length === 0) addRow();
        setInterval(saveDraft, 10000);
    });

})(jQuery, window, document);
