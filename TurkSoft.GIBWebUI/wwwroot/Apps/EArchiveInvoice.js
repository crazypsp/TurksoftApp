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

        // Kalem tablo
        linesTable: '#tblLines',

        // Diğer gridler
        despatchTable: '#irsaliye_grid',
        sellerExtraTbl: '#saticiekalan_grid',
        sellerAgentTbl: '#saticiagentekalan_grid',
        buyerExtraTbl: '#aliciekalan_grid',

        // Butonlar
        btnAddLine: '#btnAddRow, #btnNewLine, .btn-new-line',
        btnAddByText: 'button,a',
        btnDespatchAdd: '#btnDespatchAdd',
        btnSellerExtra: '#btnSellerExtraAdd',
        btnSellerAgent: '#btnSellerAgentExtraAdd',
        btnBuyerExtra: '#btnBuyerExtraAdd',
        btnSave: '#btnKaydet, #btnDraftSave, #btn_taslak',
        btnSend: '#btnSendGib, #btnGibSend, #btn_gonder',
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
        APP_KEY: 'earchive_draft_full_v1'
    };
    const VAT_MODE = { EXCL: 'HARIC', INCL: 'DAHIL' };

    // ===========================
    // YARDIMCI
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
    // RFC-4122 v4 UUID (ETTN)
    function uuidV4() {
        if (w.crypto && crypto.getRandomValues) {
            const a = new Uint8Array(16); crypto.getRandomValues(a);
            a[6] = (a[6] & 0x0f) | 0x40; a[8] = (a[8] & 0x3f) | 0x80;
            const h = [...a].map(x => x.toString(16).padStart(2, '0'));
            return `${h[0]}${h[1]}${h[2]}${h[3]}-${h[4]}${h[5]}-${h[6]}${h[7]}-${h[8]}${h[9]}-${h[10]}${h[11]}${h[12]}${h[13]}${h[14]}${h[15]}`;
        }
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8); return v.toString(16);
        });
    }
    function setEttn() { const $i = $('#txtUUID'); if ($i.length) { $i.val(uuidV4()); } }

    // ========= [PATCH] Toplam Bilgileri paneline yazıcı (ID yoksa label’dan bulur) =========
    function writeByLabelContains(text, val) {
        const $lbl = $('label, strong, span, div').filter(function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            return t.indexOf(text) >= 0;
        }).first();
        if ($lbl.length) {
            const $wrap = $lbl.closest('.form-group, .row, tr, .input-group, .col, .col-md-12, .col-md-6, .col-12');
            const $inp = $wrap.find('input, textarea, select').filter(':not([type=hidden])').last();
            if ($inp.length) $inp.val(val);
        }
    }
    function syncRightTotals(map) {
        // olası ID’ler
        setVal('#MalHizmetToplamTutar, #txtMalHizmetToplamTutar', map.raw);
        setVal('#ToplamIskonto, #txtToplamIskonto', map.disc);
        setVal('#KdvMatrah20, #txtKdvMatrah20', map.net);
        setVal('#KdvTutar20, #txtKdvTutar20', map.kdv);
        setVal('#VergilerDahilToplam, #txtVergilerDahilToplam', map.grand);
        setVal('#OdenecekToplam, #txtOdenecekToplam', map.grand);

        // label metinlerinden yakala
        writeByLabelContains('mal/hizmet toplam tutar', map.raw);
        writeByLabelContains('toplam iskonto', map.disc);
        writeByLabelContains('hesaplanan kdv matrah', map.net);
        writeByLabelContains('hesaplanan kdv (', map.kdv);
        writeByLabelContains('vergiler dahil toplam tutar', map.grand);
        writeByLabelContains('ödenecek toplam tutar', map.grand);
    }
    w.__syncRightTotals = syncRightTotals; // diğer patch’ler de erişsin

    // ===========================
    // LOOKUP
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
    // KALEM TABLO • ŞABLON & HESAP
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

        // [PATCH] Sağ panel eşitle
        syncRightTotals({
            raw: fmt(raw) + ' TL',
            disc: fmt(disc) + ' TL',
            net: fmt(net) + ' TL',
            kdv: fmt(kdv) + ' TL',
            grand: fmt(grand) + ' TL'
        });
    }

    // ====== ÖZEL: #manuel_grid’e satır ekleme ======
    function addRow_manuelGrid() {
        const $tb = $('#manuel_grid tbody');
        const $last = $tb.find('tr:last');
        if (!$last.length) {
            $tb.append(w.makeLineRowManual ? w.makeLineRowManual(1) : makeLineRow(1));
        } else {
            const $clone = $last.clone(true, true);

            $clone.find('input,select,textarea').each(function () {
                const $el = $(this);
                const type = ($el.attr('type') || '').toLowerCase();
                if (type === 'checkbox' || type === 'radio') $el.prop('checked', false);
                else $el.val('');
            });

            $clone.find('.ln-qty').val('1');
            const $unit = $clone.find('.ln-unit');
            if ($unit.is('select')) {
                const firstOpt = $unit.find('option:first').val();
                $unit.val(firstOpt || 'C62');
            } else { $unit.val('C62'); }
            $clone.find('.ln-price').val('1');
            $clone.find('.ln-discp').val('0');
            $clone.find('.ln-kdv').val(String(DEFAULTS.VAT));
            $clone.find('.ln-total,.ln-isk,.ln-net,.ln-kdvt').val('');

            const idx = $tb.find('tr').length + 1;
            $clone.find('td:first').text(idx);

            $clone.find('.js-del-line,.btnDel,.js-del-row').off('click._m').on('click._m', function () {
                $(this).closest('tr').remove();
                recalcTotals('#manuel_grid');
                saveDraft();
            });

            $tb.append($clone);
        }
        recalcTotals('#manuel_grid');
        saveDraft();
    }

    // ===========================
    // LINES TABLO BIND
    // ===========================
    function bindLineTable() {
        const linesSel = SEL.linesTable;
        const isManGrid = (linesSel === '#manuel_grid');

        function addLine() {
            if (isManGrid) addRow_manuelGrid();
            else {
                const idx = $(`${linesSel} tbody tr`).length + 1;
                $(`${linesSel} tbody`).append(makeLineRow(idx));
                recalcTotals(linesSel);
                saveDraft();
            }
        }

        // ID ile bağla
        $(SEL.btnAddLine).off('click.addline').on('click.addline', addLine);

        // Metin bazlı (kalsın ama finalizer guard’lı)
        $(d).off('click.addline_txt', SEL.btnAddByText).on('click.addline_txt', SEL.btnAddByText, function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t.indexOf('yeni') >= 0 && t.indexOf('satır') >= 0 && t.indexOf('ekle') >= 0) addLine();
        });

        // manuel_grid caption
        $(d).off('click.addline_cap_m', '#manuel_grid caption .btn, #manuel_grid caption button, #manuel_grid caption a')
            .on('click.addline_cap_m', '#manuel_grid caption .btn, #manuel_grid caption button, #manuel_grid caption a', function () {
                const t = ($(this).text() || '').trim().toLowerCase();
                if (t.indexOf('yeni') >= 0 && t.indexOf('ekle') >= 0) addRow_manuelGrid();
            });

        // sil
        $(d).off('click.delline', '.js-del-line, .btnDel').on('click.delline', '.js-del-line, .btnDel', function () {
            $(this).closest('tr').remove();
            renumberLines(linesSel);
            recalcTotals(linesSel);
            saveDraft();
        });

        // hesap
        $(d).off('input.recalc change.recalc', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad')
            .on('input.recalc change.recalc', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
                recalcTotals(linesSel);
                saveDraft();
            });

        $('#vatMode').off('change.recalc').on('change.recalc', function () {
            recalcTotals(linesSel);
            saveDraft();
        });

        if ($(`${linesSel} tbody tr`).length === 0) addLine();
    }

    // ===========================
    // BASİT GRIDLER
    // ===========================
    function simpleRowHtml(colsHtmlArr) {
        return `<tr>${colsHtmlArr.map(x => `<td>${x}</td>`).join('')}<td class="text-center"><button type="button" class="btn btn-danger btn-xs js-del-row">X</button></td></tr>`;
    }
    function bindSimpleGrid(addBtnSel, tableSel, inputs) {
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

        if ($(addBtnSel).length) $(addBtnSel).off(`click.add_${tableSel}`).on(`click.add_${tableSel}`, addRow);

        $(d).off(`click.addcap_${tableSel}`, `${tableSel} caption .btn, ${tableSel} caption button, ${tableSel} caption a`)
            .on(`click.addcap_${tableSel}`, `${tableSel} caption .btn, ${tableSel} caption button, ${tableSel} caption a`, function () {
                const txt = ($(this).text() || '').trim().toLowerCase();
                if (txt.indexOf('yeni') >= 0 && txt.indexOf('ekle') >= 0) addRow();
            });

        $(d).off(`click.del_${tableSel}`, `${tableSel} .js-del-row`).on(`click.del_${tableSel}`, `${tableSel} .js-del-row`, function () {
            $(this).closest('tr').remove();
        });
    }
    function bindAllSimpleGrids() {
        bindSimpleGrid(SEL.btnDespatchAdd, SEL.despatchTable, [
            { type: 'text', placeholder: 'İrsaliye No' },
            { type: 'text', placeholder: 'İrsaliye Tarihi' }
        ]);
        bindSimpleGrid(SEL.btnSellerExtra, SEL.sellerExtraTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);
        bindSimpleGrid(SEL.btnSellerAgent, SEL.sellerAgentTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);
        bindSimpleGrid(SEL.btnBuyerExtra, SEL.buyerExtraTbl, [
            { type: 'select', list: (w.taniticikodList || []), getVal: x => x.TaniticiKod, getText: x => x.TaniticiKod },
            { type: 'text', placeholder: 'Değer' }
        ]);

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
    // TASLAK
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
    // VERGİ ÖZET
    // ===========================
    function computeTaxes(linesSel) {
        const bag = {};
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
    // DTO
    // ===========================
    function collectInvoice() {
        const now = new Date().toISOString();
        const linesSel = SEL.linesTable;
        const currency = ($(SEL.currency).val() || DEFAULTS.CURRENCY).toUpperCase();

        const dto = {
            entity: 'EArchive',
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

        const taxes = computeTaxes(linesSel);
        Object.keys(taxes).forEach(k => {
            dto.invoicesTaxes.push({ name: 'KDV', rate: Number(k), amount: taxes[k].amount, createdAt: now, updatedAt: now });
        });

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
    // API
    // ===========================
    function getInvoiceApi() {
        if (w.InvoiceApi && typeof w.InvoiceApi.create === 'function') return w.InvoiceApi;
        return {
            create: (dto) => $.ajax({ url: '/api/v1/invoice', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' }),
            send: (dto) => $.ajax({ url: '/api/v1/invoice/send', method: 'POST', data: JSON.stringify(dto), contentType: 'application/json' })
        };
    }

    async function doSave() {
        const dto = collectInvoice();
        if (!(dto.invoicesItems || []).length) { alert('En az bir satır ekleyin.'); return; }
        try {
            const res = await getInvoiceApi().create(dto);
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
        $(SEL.btnSave).off('click.save').on('click.save', doSave);
        $(SEL.btnSend).off('click.send').on('click.send', doSend);

        $(SEL.btnPreview).off('click.prev').on('click.prev', doPreview);
        $(SEL.btnPdf).off('click.pdf').on('click.pdf', doDownloadPdf);
        $(SEL.btnXml).off('click.xml').on('click.xml', doDownloadXml);

        $(d).off('click.pdf_text', 'button,a').on('click.pdf_text', 'button,a', function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t === 'pdf indir' || t.indexOf('pdf') >= 0) doDownloadPdf();
        });
        $(d).off('click.xml_text', 'button,a').on('click.xml_text', 'button,a', function () {
            const t = ($(this).text() || '').trim().toLowerCase();
            if (t === 'xml indir' || t.indexOf('xml') >= 0) doDownloadXml();
        });

        setInterval(() => saveDraft(), 10000);
    }

    // ===========================
    // INIT
    // ===========================
    $(function () {
        console.log('🧾 Invoice.js (final+patch) yüklendi');

        SEL.linesTable = (document.getElementById('manuel_grid') ? '#manuel_grid'
            : (document.getElementById('tblLines') ? '#tblLines'
                : (document.getElementById('lines') ? '#lines' : '#tblLines')));

        $('.help-block, small').filter(function () {
            const t = ($(this).text() || '').toLowerCase();
            return t.indexOf('nox') >= 0 && t.indexOf('yazılım') >= 0;
        }).hide();

        safeInitPickers();
        loadLookups();
        bindLineTable();
        bindAllSimpleGrids();
        bindActions();
        setEttn();              // ← ETTN oluştur
        loadDraft();

        $('button, a').filter(function () { return ($(this).text() || '').trim().toLowerCase().indexOf('yeni satır ekle') >= 0; })
            .addClass('btn-new-line');

        w.EinvoiceUI = { collectInvoice, recalcTotals, saveDraft, applyDraft };
    });

    /* ==== BEGIN: EInvoice patch (non-destructive, append-only) ==== */
    (function ($, w, d) {
        'use strict';

        if (w.__EINV_PATCH_APPLIED__) return;
        w.__EINV_PATCH_APPLIED__ = true;

        if (typeof w.fixLinesHeaderAlignment !== 'function') {
            w.fixLinesHeaderAlignment = function fixLinesHeaderAlignment() {
                try {
                    var $tbl = $('#tblLines').length ? $('#tblLines')
                        : ($('#lines').length ? $('#lines')
                            : ($('#manuel_grid').length ? $('#manuel_grid') : $()));
                    if (!$tbl.length) return;

                    var $head = $tbl.find('thead th');
                    var $row = $tbl.find('tbody tr:visible:first');
                    if (!$head.length || !$row.length) return;

                    var $tds = $row.children('td');
                    if ($tds.length !== $head.length) return;

                    $tds.each(function (i) {
                        var w = $(this).outerWidth();
                        $($head[i]).css('width', w);
                    });
                } catch (e) { }
            };
        }

        function ensureRowActionButtons($tbl) {
            try {
                $tbl.find('tbody tr').each(function () {
                    var $last = $(this).children('td').last();
                    if (!$last.length) return;

                    var hasGroup = $last.find('.einv-btn-group').length > 0;
                    if (!hasGroup) {
                        var $del = $last.find('.js-del-line, .btnDel').first();
                        var $grp = $('<div class="btn-group btn-group-xs einv-btn-group" role="group" style="margin-left:4px;"></div>');

                        $('<button type="button" class="btn btn-success js-line-add" title="Altına Satır Ekle"><i class="fa fa-plus"></i></button>')
                            .appendTo($grp);

                        $('<button type="button" class="btn btn-warning js-line-edit" title="Düzenle"><i class="fa fa-pencil"></i></button>')
                            .appendTo($grp);

                        if ($del.length) {
                            $del.after($grp);
                        } else {
                            $last.append($grp);
                            $('<button type="button" class="btn btn-danger js-del-line" title="Sil" style="margin-left:4px;"><i class="fa fa-trash"></i></button>').appendTo($grp);
                        }
                    }
                });
            } catch (e) { }
        }

        function triggerTotals() {
            try {
                if (typeof w.recalcTotals === 'function') {
                    try { w.recalcTotals((w.SEL && w.SEL.linesTable) || '#tblLines'); }
                    catch { w.recalcTotals(); }
                } else if (typeof w.recalc === 'function') {
                    w.recalc();
                }
            } catch (e) { }
        }

        function triggerRenumber() {
            try {
                if (typeof w.renumberLines === 'function') {
                    w.renumberLines((w.SEL && w.SEL.linesTable) || '#tblLines');
                } else if (typeof w.renumber === 'function') {
                    w.renumber();
                }
            } catch (e) { }
        }

        $(d)
            .off('click.einv.add', '.js-line-add')
            .on('click.einv.add', '.js-line-add', function () {
                var $tr = $(this).closest('tr');
                var $tbl = $tr.closest('table');
                var idx = $tbl.find('tbody tr').length + 1;

                try {
                    if (typeof w.rowTemplate === 'function') {
                        $tr.after(w.rowTemplate(idx));
                    } else if (typeof w.makeLineRow === 'function') {
                        $tr.after(w.makeLineRow(idx));
                    } else {
                        var $clone = $tr.clone(true, true);
                        $clone.find('.ln-qty').val('1');
                        $clone.find('.ln-price').val('1');
                        $clone.find('.ln-discp').val('0');
                        $clone.find('.ln-kdv').val($clone.find('.ln-kdv').val() || '20');
                        $clone.find('.ln-total').val('');
                        $tr.after($clone);
                    }
                } catch { }

                triggerRenumber();
                triggerTotals();
                w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                ensureRowActionButtons($tbl);
            })

            .off('click.einv.edit', '.js-line-edit')
            .on('click.einv.edit', '.js-line-edit', function () {
                var $tr = $(this).closest('tr');
                var $eds = $tr.find('.ln-ad,.ln-qty,.ln-unit,.ln-price,.ln-discp,.ln-kdv');
                var disabled = $eds.prop('disabled');
                $eds.prop('disabled', !disabled);
                if (!disabled) { $eds.first().focus(); }
            })

            .off('click.einv.del.after', '.js-del-line, .btnDel')
            .on('click.einv.del.after', '.js-del-line, .btnDel', function () {
                setTimeout(function () {
                    triggerRenumber();
                    triggerTotals();
                    w.fixLinesHeaderAlignment && w.fixLinesHeaderAlignment();
                }, 0);
            })

            .off('input.einv change.einv', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad')
            .on('input.einv change.einv', '.ln-qty,.ln-price,.ln-discp,.ln-kdv,.ln-unit,.ln-ad', function () {
                triggerTotals();
            });

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




/* ==== BEGIN: DataAnalyst Final Patch v2 ==== */
(function ($, w, d) {
    'use strict';

    var lists = {
        invoicemodel: w.invoicemodel,
        birimList: w.birimList,
        istisnaList: w.istisnaList,
        vergiList: w.vergiList,
        tevkifatList: w.tevkifatList,
        parabirimList: w.parabirimList,
        teslimsartiList: w.teslimsartiList,
        kabcinsiList: w.kabcinsiList,
        gonderimsekliList: w.gonderimsekliList,
        taniticikodList: w.taniticikodList,
        ulkeList: w.ulkeList,
        ilList: w.ilList,
        ilceList: w.ilceList,
        subeList: w.subeList,
        GondericiEtiketList: w.GondericiEtiketList,
        KurumEtiketList: w.KurumEtiketList,
        GetXsltList: w.GetXsltList,
        OdemeKanalList: w.OdemeKanalList,
        OdemeList: w.OdemeList,
        OdemeEArsivList: w.OdemeEArsivList,
        kdvOranKontrol: w.kdvOranKontrol
    };

    var __origLoadLookups = (typeof loadLookups === 'function') ? loadLookups : null;
    w.loadLookups = function () {
        if (__origLoadLookups) try { __origLoadLookups(); } catch (e) { }

        function fillSelect($el, list, getVal, getText, placeholder) {
            if (!$el || !$el.length) return;
            $el.empty();
            if (placeholder) $el.append($('<option/>', { value: '', text: placeholder }));
            (list || []).forEach(x => $el.append($('<option/>', { value: getVal(x), text: getText(x) })));
            $el.trigger('change');
        }

        (function () {
            var $sel = $('#txtInvoice_Prefix'); if (!$sel.length) return;
            var prefixList = [];
            try { if (lists.invoicemodel?.invoiceheader?.Prefix) prefixList.push(lists.invoicemodel.invoiceheader.Prefix); } catch { }
            ['INV', 'TS', 'NYZ'].forEach(p => { if (!prefixList.includes(p)) prefixList.push(p); });
            fillSelect($sel, prefixList, x => x, x => x, 'Seçiniz');
        })();

        (function () {
            var $sel = $('#DocumentCurrencyCode'); if (!$sel.length) return;
            if (lists.parabirimList) {
                fillSelect($sel, lists.parabirimList, x => x.Kodu, x => `${x.Kodu} — ${x.Aciklama}`, 'Seçiniz');
                if (!$sel.val()) $sel.val('TRY').trigger('change');
            } else {
                fillSelect($sel, [{ Kodu: 'TRY', Aciklama: 'Türk Lirası' }], x => x.Kodu, x => `${x.Kodu} — ${x.Aciklama}`, 'Seçiniz');
                $sel.val('TRY').trigger('change');
            }
        })();

        (function () {
            if (lists.ulkeList) fillSelect($('#selulke'), lists.ulkeList, x => x.UlkeKodu || x.UlkeAdi, x => x.UlkeAdi, 'Seçiniz');
            if (lists.ilList) fillSelect($('#selIl'), lists.ilList, x => x.IlAdi, x => x.IlAdi, 'Seçiniz');
            $('#selIl').off('change.patch_ilce').on('change.patch_ilce', function () {
                var il = $(this).val();
                if (lists.ilceList) {
                    var rows = lists.ilceList.filter(x => x.IlAdi === il);
                    fillSelect($('#selIlce'), rows, x => x.IlceAdi, x => x.IlceAdi, 'Seçiniz');
                }
            });
        })();

        if (lists.GetXsltList) fillSelect($('#ddlxsltdosyasi'), lists.GetXsltList, x => x, x => x, 'Seçiniz');

        if (lists.GondericiEtiketList) fillSelect($('#SourceUrn'), lists.GondericiEtiketList, x => x, x => x, 'Seçiniz');
        if (lists.KurumEtiketList) fillSelect($('#DestinationUrn'), lists.KurumEtiketList, x => x, x => x, 'Seçiniz');

        if (lists.OdemeList) fillSelect($('#PaymentMeansCode'), lists.OdemeList, x => x.OdemeKodu, x => `${x.OdemeKodu} — ${x.Aciklama}`, 'Seçiniz');

        if (lists.gonderimsekliList) fillSelect($('#EArchiveSendType'), lists.gonderimsekliList, x => x.Kodu || x.Kod || x.Value || x, x => x.Aciklama || x.Text || x, 'Seçiniz');

        (function () {
            var $m = $('#kdvStatu'); if (!$m.length) return;
            if ($m.children().length === 0) {
                $m.append('<option value="HARIC">KDV Hariç</option><option value="DAHIL">KDV Dahil</option>');
                $m.val('HARIC');
            }
        })();

        w.__unitList = (lists.birimList || []).map(b => ({ ShortName: b.BirimKodu, Name: b.Aciklama }));
    };

    w.makeLineRowManual = function (idx) {
        function unitOpts() {
            var list = w.__unitList && w.__unitList.length ? w.__unitList : [{ ShortName: 'C62', Name: 'ADET' }];
            return list.map(u => `<option value="${u.ShortName}">${u.Name} - ${u.ShortName}</option>`).join('');
        }
        function istisnaOpts() {
            var list = lists.istisnaList || [];
            var o = ['<option value="">— Seçiniz —</option>'];
            list.forEach(x => o.push(`<option value="${x.Kodu || x.Code || ''}">${(x.Kodu || '')} — ${(x.Adi || x.Name || '')}</option>`));
            return o.join('');
        }
        return `
      <tr>
        <td class="text-center">${idx}</td>
        <td><input class="form-control ln-ad" placeholder="Mal/Hizmet Adı" value=""></td>
        <td><input type="number" min="0" step="0.0001" class="form-control ln-qty" value="1"></td>
        <td><select class="form-control ln-unit">${unitOpts()}</select></td>
        <td><input type="number" step="0.01" class="form-control ln-price" value="1"></td>
        <td><input type="number" step="0.01" class="form-control ln-discp" value="0"></td>
        <td><input class="form-control ln-isk" readonly value="0,00"></td>
        <td><input class="form-control ln-net" readonly value="1,00"></td>
        <td><select class="form-control ln-kdv"><option>0</option><option>8</option><option>10</option><option>12</option><option>18</option><option selected>20</option></select></td>
        <td><input class="form-control ln-kdvt" readonly value="0,20"></td>
        <td><select class="form-control ln-istisna">${istisnaOpts()}</select></td>
        <td class="text-center">
          <button type="button" class="btn btn-xs btn-danger js-del-line" title="Sil"><i class="fa fa-trash"></i></button>
        </td>
      </tr>`;
    };

    w.recalcTotals = (function (orig) {
        function fmt(n) { n = (Math.round((n ?? 0) * 100) / 100).toFixed(2); var p = n.split('.'); p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.'); return p.join(','); }
        function dec(v) { v = ('' + (v ?? '')).replace(' TL', '').trim().replace(/\./g, '').replace(',', '.'); var n = parseFloat(v); return Number.isFinite(n) ? n : 0; }
        function getVatMode() {
            const $m = $('#kdvStatu, #vatMode').first();
            const t = ($m.val() || '').toString().toUpperCase();
            return t.indexOf('DAH') >= 0 ? 'DAHIL' : 'HARIC';
        }

        return function (linesSel) {
            linesSel = linesSel || (w.SEL && w.SEL.linesTable) || '#tblLines';
            var useManual = (linesSel === '#manuel_grid');

            var raw = 0, disc = 0, net = 0, kdv = 0, grand = 0;
            $(`${linesSel} tbody tr`).each(function () {
                var $r = $(this);
                var qty = dec($r.find('.ln-qty').val());
                var price = dec($r.find('.ln-price').val());
                var discp = dec($r.find('.ln-discp').val());
                var rate = dec($r.find('.ln-kdv').val()) || 0;

                var tutar = qty * price;
                if (getVatMode() === 'DAHIL') {
                    var unitNet = price / (1 + rate / 100);
                    tutar = qty * unitNet;
                }
                var iskT = tutar * (discp / 100);
                var base = tutar - iskT;
                var kdvt = base * (rate / 100);
                var gross = base + kdvt;

                raw += qty * price;
                disc += iskT;
                net += base;
                kdv += kdvt;
                grand += gross;

                if (useManual) {
                    $r.find('.ln-isk').val(fmt(iskT));
                    $r.find('.ln-net').val(fmt(base));
                    $r.find('.ln-kdvt').val(fmt(kdvt));
                } else {
                    if ($r.find('.ln-total').length) $r.find('.ln-total').val(fmt(gross));
                }
            });

            function setVal2(sel, val) {
                var $el = $(sel);
                if ($el.length) { if ($el.is('input,select,textarea')) $el.val(val); else $el.text(val); }
            }
            setVal2('#tAra', fmt(raw) + ' TL');
            setVal2('#tIsk', fmt(disc) + ' TL');
            setVal2('#tMatrah20', fmt(net) + ' TL');
            setVal2('#tKdv20', fmt(kdv) + ' TL');
            setVal2('#tGenel', fmt(grand) + ' TL');

            // [PATCH] Sağ paneli de güncelle
            if (w.__syncRightTotals) w.__syncRightTotals({
                raw: fmt(raw) + ' TL',
                disc: fmt(disc) + ' TL',
                net: fmt(net) + ' TL',
                kdv: fmt(kdv) + ' TL',
                grand: fmt(grand) + ' TL'
            });

            var $topSel = $('#manuelToplam');
            if ($topSel.length) {
                var opts = [
                    { v: 'ARA', t: `Ara Toplam: ${fmt(raw)} TL` },
                    { v: 'ISK', t: `İskonto: ${fmt(disc)} TL` },
                    { v: 'MATRAH', t: `Matrah: ${fmt(net)} TL` },
                    { v: 'KDV', t: `KDV: ${fmt(kdv)} TL` },
                    { v: 'GENEL', t: `Genel Toplam: ${fmt(grand)} TL` },
                ];
                $topSel.empty(); opts.forEach(o => $topSel.append($('<option/>', { value: o.v, text: o.t })));
            }
        };
    })(w.recalcTotals);

    w.addRow_manuelGrid = (function (origFn) {
        return function () {
            var $tb = $('#manuel_grid tbody');
            if (!$tb.length) return;
            var $last = $tb.find('tr:last');
            if (!$last.length) {
                $tb.append(w.makeLineRowManual(1));
            } else {
                var $clone = $last.clone(true, true);
                $clone.find('input,select,textarea').each(function () {
                    var $el = $(this), type = ($el.attr('type') || '').toLowerCase();
                    if (type === 'checkbox' || type === 'radio') $el.prop('checked', false); else $el.val('');
                });
                $clone.find('.ln-qty').val('1'); $clone.find('.ln-price').val('1'); $clone.find('.ln-discp').val('0'); $clone.find('.ln-kdv').val('20');
                $clone.find('.ln-isk,.ln-net,.ln-kdvt').val('');
                var idx = $tb.find('tr').length + 1;
                $clone.find('td:first').text(idx);
                $tb.append($clone);
            }
            (w.recalcTotals || function () { })('#manuel_grid');
            (w.saveDraft || function () { })();
        };
    })(w.addRow_manuelGrid || null);

    // (ESKİ) metne göre ekleme — çakışma yapabiliyor; final patch'te devre dışı bırakılacak
    $(d).off('click.da_addline', 'button,a').on('click.da_addline', 'button,a', function () {
        var t = ($(this).text() || '').trim().toLowerCase();
        if (t.indexOf('yeni') >= 0 && t.indexOf('satır') >= 0 && t.indexOf('ekle') >= 0) {
            if ($('#manuel_grid').length) { w.addRow_manuelGrid(); }
        }
    });

    w.collectInvoiceInvoiceCs = function () {
        var now = new Date().toISOString();
        var currency = ($('#DocumentCurrencyCode').val() || 'TRY').toUpperCase();
        var linesSel = (w.SEL && w.SEL.linesTable) || (document.getElementById('manuel_grid') ? '#manuel_grid' : '#tblLines');

        function dec(v) { v = ('' + (v ?? '')).replace(' TL', '').trim().replace(/\./g, '').replace(',', '.'); var n = parseFloat(v); return Number.isFinite(n) ? n : 0; }
        function getVatMode() { var t = ($('#kdvStatu,#vatMode').first().val() || '').toUpperCase(); return t.indexOf('DAH') >= 0 ? 'DAHIL' : 'HARIC'; }
        function fmtTotals() {
            var raw = 0, disc = 0, net = 0, kdv = 0, grand = 0;
            $(`${linesSel} tbody tr`).each(function () {
                var $r = $(this);
                var qty = dec($r.find('.ln-qty').val());
                var price = dec($r.find('.ln-price').val());
                var discp = dec($r.find('.ln-discp').val());
                var rate = dec($r.find('.ln-kdv').val()) || 0;
                var tutar = qty * price;
                if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
                var iskT = tutar * (discp / 100);
                var base = tutar - iskT;
                var kdvt = base * (rate / 100);
                raw += qty * price; disc += iskT; net += base; kdv += kdvt; grand += base + kdvt;
            });
            return { raw, disc, net, kdv, grand };
        }

        var dto = {
            InvoiceNo: ($('#txtInvoice_Prefix').val() || 'INV') + '-' + Date.now(),
            InvoiceDate: now,
            Currency: currency,
            Total: 0,
            Customer: {
                Name: $('#txtPartyName').val() || '',
                Surname: $('#txtPerson_FamilyName').val() || '',
                Email: $('#txtElectronicMail').val() || '',
                TaxNo: $('#txtIdentificationID').val() || '',
                TaxOffice: $('#txtTaxSchemeName').val() || ''
            },
            InvoicesItems: [],
            InvoicesTaxes: [],
            InvoicesDiscounts: [],
            InvoicesPayments: []
        };

        $(`${linesSel} tbody tr`).each(function () {
            var $r = $(this);
            var qty = dec($r.find('.ln-qty').val());
            var price = dec($r.find('.ln-price').val());
            var discp = dec($r.find('.ln-discp').val());
            var rate = dec($r.find('.ln-kdv').val()) || 0;
            var unitShort = ($r.find('.ln-unit').val() || 'C62').toUpperCase();
            var unitName = (unitShort === 'C62' ? 'ADET' : unitShort);
            var tutar = qty * price;
            if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
            var base = tutar - (tutar * (discp / 100));
            var gross = base * (1 + rate / 100);

            dto.InvoicesItems.push({
                Quantity: qty,
                Price: price,
                Total: gross,
                Item: {
                    Name: $r.find('.ln-ad').val() || 'GENEL ÜRÜN',
                    Code: 'ITEM-' + Math.floor(Math.random() * 100000),
                    Currency: currency,
                    Unit: { ShortName: unitShort, Name: unitName }
                }
            });
        });

        var bag = {};
        $(`${linesSel} tbody tr`).each(function () {
            var $r = $(this); var qty = dec($r.find('.ln-qty').val()); var price = dec($r.find('.ln-price').val()); var discp = dec($r.find('.ln-discp').val()); var rate = dec($r.find('.ln-kdv').val()) || 0;
            var tutar = qty * price; if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
            var base = tutar - (tutar * (discp / 100)); var kdvt = base * (rate / 100);
            if (!bag[rate]) bag[rate] = { amount: 0 };
            bag[rate].amount += kdvt;
        });
        Object.keys(bag).forEach(k => dto.InvoicesTaxes.push({ Name: 'KDV', Rate: Number(k), Amount: bag[k].amount }));

        (function () {
            var totalDisc = 0;
            $(`${linesSel} tbody tr`).each(function () {
                var $r = $(this); var qty = dec($r.find('.ln-qty').val()); var price = dec($r.find('.ln-price').val()); var discp = dec($r.find('.ln-discp').val()); var rate = dec($r.find('.ln-kdv').val()) || 0;
                var tutar = qty * price; if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
                totalDisc += tutar * (discp / 100);
            });
            dto.InvoicesDiscounts.push({ Name: 'Toplam İskonto', Desc: 'Otomatik', Base: 'Ara Toplam', Rate: 0, Amount: totalDisc });
        })();

        dto.InvoicesPayments.push({
            Payment: {
                Amount: 0, Currency: currency, Date: now, Note: $('#InstructionNote').val() || '',
                PaymentType: { Name: $('#PaymentMeansCode option:selected').text() || '' },
                PaymentAccount: { Name: $('#PayeeFinancialAccount').val() || '' },
                Bank: { Name: $('#txtBankName').val() || '' }
            }
        });

        var t = fmtTotals(); dto.Total = t.grand; dto.InvoicesPayments[0].Payment.Amount = dto.Total;

        // [PATCH] Sağ panel eşitle
        if (w.__syncRightTotals) w.__syncRightTotals({
            raw: (t.raw).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ',') + ' TL',
            disc: (t.disc).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ',') + ' TL',
            net: (t.net).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ',') + ' TL',
            kdv: (t.kdv).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ',') + ' TL',
            grand: (t.grand).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ',') + ' TL'
        });

        return dto;
    };

    $(function () {
        var selSave = '#btnKaydet, #btnDraftSave, #btn_taslak';
        $(document).off('click.da_save', selSave).on('click.da_save', selSave, function (e) {
            var model = w.collectInvoiceInvoiceCs();
            try {
                localStorage.setItem('GIBEntityDB.Invoice', JSON.stringify(model));
                w.InvoiceDraft = model;
                console.log('GIBEntityDB.Invoice → localStorage’a yazıldı', model);
            } catch { }
        });
    });

})(jQuery, window, document);
/* ==== END: DataAnalyst Final Patch v2 ==== */


/* ==== BEGIN: DataAnalyst Patch: KDV dropdown + İstisna + Totals + Modals + Responsive + HEADER/BODY NORMALIZER ==== */
(function ($, w, d) {
    'use strict';

    var VAT_RATES = [0, 8, 10, 12, 18, 20];

    // ===== Robust header/body normalizer (sütun kayması & çoğalma fix) =====
    var HEADER_HTML = [
        '<tr style="background:#ebedf0">',
        '<th style="min-width:60px;max-width:60px;white-space:nowrap;">Sıra No</th>',
        '<th style="min-width:180px;max-width:250px;white-space:nowrap;">Mal/Hizmet Adı *</th>',
        '<th style="min-width:100px;max-width:100px;white-space:nowrap;">Miktar</th>',
        '<th style="min-width:120px;max-width:160px;white-space:nowrap;">Birim *</th>',
        '<th style="min-width:120px;max-width:160px;white-space:nowrap;">Birim Fiyat</th>',
        '<th style="min-width:120px;max-width:140px;white-space:nowrap;">İskonto Oranı(%)</th>',
        '<th style="min-width:120px;max-width:140px;white-space:nowrap;">İskonto Tutarı</th>',
        '<th style="min-width:120px;max-width:140px;white-space:nowrap;">Mal/Hizmet Tutarı</th>',
        '<th style="min-width:90px;max-width:100px;text-align:center;white-space:nowrap;">KDV Oranı(%)</th>',
        '<th style="min-width:120px;max-width:140px;white-space:nowrap;">KDV Tutarı</th>',
        '<th style="min-width:160px;max-width:210px;white-space:nowrap;">İstisna</th>',
        '<th style="min-width:160px;max-width:180px;white-space:nowrap;">İşlem</th>',
        '</tr>'
    ].join('');

    function ensureSingleSections() {
        var $tbl = $('#manuel_grid'); if (!$tbl.length) return;
        var $ths = $tbl.children('thead'); if ($ths.length === 0) { $tbl.prepend('<thead></thead>'); } else if ($ths.length > 1) { $ths.slice(1).remove(); }
        var $tbs = $tbl.children('tbody'); if ($tbs.length === 0) { $tbl.append('<tbody></tbody>'); } else if ($tbs.length > 1) { $tbs.slice(1).remove(); }
    }
    function normalizeHeader() {
        var $tbl = $('#manuel_grid'); if (!$tbl.length) return;
        ensureSingleSections();
        var $thead = $tbl.children('thead');
        var marker = $tbl.data('structured') === 1;
        if (!marker || $thead.find('th').length !== 12) {
            $thead.empty().append(HEADER_HTML);
            $tbl.data('structured', 1);
        }
    }
    function rowHtml(i, v) {
        function unitOpts() {
            var list = (w.__unitList || []); if (!list.length) list = [{ ShortName: 'C62', Name: 'ADET' }];
            return list.map(u => `<option value="${u.ShortName}">${u.Name} - ${u.ShortName}</option>`).join('');
        }
        function istisnaOpts() {
            var list = (w.istisnaList || []), o = ['<option value="">— Seçiniz —</option>'];
            list.forEach(x => { var k = x.Kodu || x.Code || ''; var a = x.Adi || x.Name || ''; o.push(`<option value="${k}">${k ? (k + ' — ') : ''}${a}</option>`); });
            return o.join('');
        }
        var kdvOpts = VAT_RATES.map(r => `<option value="${r}" ${+v.rate === r ? 'selected' : ''}>${r}</option>`).join('');
        return [
            '<tr>',
            `<td class="text-center" style="white-space:nowrap;">${i}</td>`,
            `<td style="white-space:nowrap;"><input class="form-control ln-ad" placeholder="Mal/Hizmet Adı" value="${v.ad || 'GENEL ÜRÜN'}"></td>`,
            `<td style="white-space:nowrap;"><input type="number" min="0" step="0.0001" class="form-control ln-qty" value="${v.qty ?? 1}" style="max-width:110px;"></td>`,
            `<td style="white-space:nowrap;"><select class="form-control ln-unit" style="max-width:160px;">${unitOpts()}</select></td>`,
            `<td style="white-space:nowrap;"><input type="number" step="0.01" class="form-control ln-price" value="${v.price ?? 1}" style="max-width:110px;"></td>`,
            `<td style="white-space:nowrap;"><input type="number" step="0.01" class="form-control ln-discp" value="${v.discp ?? 0}" style="max-width:110px;"></td>`,
            `<td style="white-space:nowrap;"><input class="form-control ln-isk" readonly value="${v.isk || '0,00'}" style="max-width:140px;"></td>`,
            `<td style="white-space:nowrap;"><input class="form-control ln-net" readonly value="${v.net || '0,00'}" style="max-width:140px;"></td>`,
            `<td style="white-space:nowrap;"><select class="form-control ln-kdv" style="max-width:90px;">${kdvOpts}</select></td>`,
            `<td style="white-space:nowrap;"><input class="form-control ln-kdvt" readonly value="${v.kdvt || '0,00'}" style="max-width:140px;"></td>`,
            `<td style="white-space:nowrap;"><select class="form-control ln-istisna" style="max-width:210px;">${istisnaOpts()}</select></td>`,
            `<td class="text-center" style="white-space:nowrap;"><button type="button" class="btn btn-xs btn-danger js-del-line"><i class="fa fa-trash"></i></button></td>`,
            '</tr>'
        ].join('');
    }
    function normalizeBody() {
        var $tbl = $('#manuel_grid'); if (!$tbl.length) return;
        ensureSingleSections();
        var $tb = $tbl.children('tbody'); if (!$tb.length) return;
        var rows = [];
        $tb.find('tr').each(function () {
            var $r = $(this);
            rows.push({
                ad: $r.find('.ln-ad').val(),
                qty: +($r.find('.ln-qty').val() || 1),
                unit: ($r.find('.ln-unit').val() || 'C62'),
                price: +($r.find('.ln-price').val() || 1),
                discp: +($r.find('.ln-discp').val() || 0),
                rate: +($r.find('.ln-kdv').val() || 20),
                isk: $r.find('.ln-isk').val(),
                net: $r.find('.ln-net').val(),
                kdvt: $r.find('.ln-kdvt').val(),
                istis: $r.find('.ln-istisna').val()
            });
        });
        if (!rows.length) rows.push({});
        $tb.empty();
        rows.forEach(function (v, i) {
            var html = rowHtml(i + 1, v); $tb.append(html);
            var $row = $tb.find('tr:last');
            $row.find('.ln-unit').val(v.unit || 'C62');
            $row.find('.ln-istisna').val(v.istis || '');
        });
    }

    function ensureHeaders() { normalizeHeader(); }
    function ensureBody() { normalizeBody(); }

    function getVatMode() {
        var t = ($('#kdvStatu,#vatMode').first().val() || '').toUpperCase();
        return t.indexOf('DAH') >= 0 ? 'DAHIL' : 'HARIC';
    }
    function dec(v) { v = ('' + (v ?? '')).replace(' TL', '').trim().replace(/\./g, '').replace(',', '.'); var n = parseFloat(v); return Number.isFinite(n) ? n : 0; }
    function fmt(n) { n = (Math.round((n ?? 0) * 100) / 100).toFixed(2); var p = n.split('.'); p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.'); return p.join(','); }

    function setIstisnaEnabled() {
        var en = (($('#ddlfaturatip').val() || '').toUpperCase() === 'ISTISNA');
        $('#manuel_grid .ln-istisna').prop('disabled', !en);
    }

    function makeResponsive() {
        var $tbl = $('#manuel_grid'); if (!$tbl.length) return;
        if (!$tbl.parent().hasClass('table-responsive')) {
            $tbl.wrap('<div class="table-responsive" style="overflow-x:auto;"></div>');
        }
        $tbl.find('th,td').css({ whiteSpace: 'nowrap' });
        $tbl.find('.ln-qty,.ln-price,.ln-discp').css({ maxWidth: '110px' });
        $tbl.find('.ln-kdvt').css({ maxWidth: '140px' });
        $tbl.find('.ln-istisna').css({ maxWidth: '210px' });
    }

    function updateRowKdvt() {
        $('#manuel_grid tbody tr').each(function () {
            var $r = $(this);
            var qty = dec($r.find('.ln-qty').val());
            var price = dec($r.find('.ln-price').val());
            var discp = dec($r.find('.ln-discp').val());
            var rate = dec($r.find('.ln-kdv').val()) || 0;

            var tutar = qty * price;
            if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
            var base = tutar - (tutar * (discp / 100));
            var kdvt = base * (rate / 100);
            $r.find('.ln-kdvt').val(fmt(kdvt));
        });
    }

    function fillTotalsPanel() {
        var raw = 0, disc = 0, net = 0, kdv = 0, grand = 0;
        $('#manuel_grid tbody tr').each(function () {
            var $r = $(this);
            var qty = dec($r.find('.ln-qty').val());
            var price = dec($r.find('.ln-price').val());
            var discp = dec($r.find('.ln-discp').val());
            var rate = dec($r.find('.ln-kdv').val()) || 0;
            var tutar = qty * price;
            if (getVatMode() === 'DAHIL') { var unitNet = price / (1 + rate / 100); tutar = qty * unitNet; }
            var iskT = tutar * (discp / 100);
            var base = tutar - iskT;
            var kdvt = base * (rate / 100);
            raw += qty * price; disc += iskT; net += base; kdv += kdvt; grand += base + kdvt;
        });

        function setValX(sel, val) { var $el = $(sel); if ($el.length) { if ($el.is('input,select,textarea')) $el.val(val); else $el.text(val); } }
        setValX('#tAra', fmt(raw) + ' TL');
        setValX('#tIsk', fmt(disc) + ' TL');
        setValX('#tMatrah20', fmt(net) + ' TL');
        setValX('#tKdv20', fmt(kdv) + ' TL');
        setValX('#tGenel', fmt(grand) + ' TL');

        // [PATCH] Sağ panel
        if (w.__syncRightTotals) w.__syncRightTotals({
            raw: fmt(raw) + ' TL',
            disc: fmt(disc) + ' TL',
            net: fmt(net) + ' TL',
            kdv: fmt(kdv) + ' TL',
            grand: fmt(grand) + ' TL'
        });
    }

    var __origRecalc = w.recalcTotals;
    w.recalcTotals = function (linesSel) {
        normalizeHeader();
        normalizeBody();
        if (typeof __origRecalc === 'function') __origRecalc(linesSel || '#manuel_grid');
        updateRowKdvt();
        fillTotalsPanel();
        setIstisnaEnabled();
        makeResponsive();
    };

    function findModalFor($btn) {
        var sel = $btn.attr('data-target') || $btn.attr('data-bs-target') || $btn.attr('href');
        if (sel && sel.startsWith('#') && $(sel).hasClass('modal')) return $(sel);
        var txt = ($btn.text() || '').toLowerCase();
        var key = null;
        if (txt.indexOf('medula') >= 0) key = 'medula';
        else if (txt.indexOf('cezaevi') >= 0) key = 'ceza';
        else if (txt.indexOf('toplu') >= 0 && (txt.indexOf('indirim') >= 0 || txt.indexOf('artırım') >= 0 || txt.indexOf('artirim') >= 0)) key = 'iskonto';
        else if (txt.indexOf('toplu') >= 0 && txt.indexOf('vergi') >= 0) key = 'vergi';
        if (key) {
            var $m = $('.modal[id*="' + key + '"]'); if ($m.length) return $m.first();
        }
        return $();
    }

    $(function () {
        normalizeHeader(); normalizeBody(); makeResponsive(); setIstisnaEnabled();
        updateRowKdvt(); fillTotalsPanel();

        $(d).on('change', '#ddlfaturatip', setIstisnaEnabled);

        $(d).on('click', 'button,a', function () {
            var t = ($(this).text() || '').toLowerCase();
            if (t.includes('yeni') && t.includes('satır') && t.includes('ekle')) {
                setTimeout(function () { normalizeBody(); setIstisnaEnabled(); makeResponsive(); w.recalcTotals && w.recalcTotals('#manuel_grid'); }, 0);
            }
        });

        $(d).on('input change', '#manuel_grid .ln-qty, #manuel_grid .ln-price, #manuel_grid .ln-discp, #manuel_grid .ln-kdv, #kdvStatu, #vatMode',
            function () { w.recalcTotals && w.recalcTotals('#manuel_grid'); });

        // [PATCH] Modallar — güvenli aç
        $(d).on('click', 'button,a', function (e) {
            var t = ($(this).text() || '').toLowerCase();
            if (t.indexOf('medula') >= 0 || t.indexOf('cezaevi') >= 0 || (t.indexOf('toplu') >= 0 && (t.indexOf('indirim') >= 0 || t.indexOf('artırım') >= 0 || t.indexOf('artirim') >= 0)) || (t.indexOf('toplu') >= 0 && t.indexOf('vergi') >= 0)) {
                var $m = findModalFor($(this));
                if ($m.length) { e.preventDefault(); try { $m.modal('show'); } catch { $m.show(); } }
            }
        });
    });

})(jQuery, window, document);
/* ==== END: DataAnalyst Patch (Normalized) ==== */


/* ==== BEGIN: Manuel Grid Finalizer (toolbar KDV Dahil/Hariç + double-add fix) ==== */
(function ($, w, d) {
    'use strict';

    // 1) ÇAKIŞAN metin-yakalayıcıyı kaldır, güvenli olanla değiştir
    $(d).off('click.da_addline', 'button,a');
    $(d).off('click.addline_txt', 'button,a')
        .on('click.addline_txt', 'button,a', function (e) {
            var t = ($(this).text() || '').trim().toLowerCase();
            if (t.indexOf('yeni') >= 0 && t.indexOf('satır') >= 0 && t.indexOf('ekle') >= 0) {
                if ($(this).is('#btnAddRow, #btnNewLine, .btn-new-line')) return; // ikinci tetikleme olmasın
                if (!$('#manuel_grid').length) return;
                e.preventDefault();
                (w.addRow_manuelGrid || function () { })();
                setTimeout(function () { (w.recalcTotals || function () { })('#manuel_grid'); }, 0);
            }
        });

    // 2) Excel yanına "KDV Durumu" select — [PATCH] buton grubunun SAĞINA ekle
    function ensureVatModeToolbar() {
        if ($('#vatMode').length) return;

        var html = [
            '<div class="btn-group" id="vatModeWrap" style="margin-left:6px;vertical-align:top;">',
            '<select id="vatMode" class="form-control" style="height:30px;min-width:130px;padding:2px 6px;">',
            '<option value="DAHIL">KDV Dahil</option>',
            '<option value="HARIC">KDV Hariç</option>',
            '</select>',
            '</div>'
        ].join('');

        var $excelBtn = $('button,a').filter(function () { return ($(this).text() || '').toLowerCase().trim() === 'excel'; }).last();
        var $grp = $excelBtn.closest('.btn-group');
        if ($grp.length) $grp.after(html);
        else if ($excelBtn.length) $excelBtn.after(html);
        else $(html).insertBefore('#manuel_grid');

        // varsa kdvStatu’ya senkron
        var cur = ($('#kdvStatu').val() || '').toUpperCase();
        if (cur) $('#vatMode').val(cur.indexOf('DAH') >= 0 ? 'DAHIL' : 'HARIC');

        $('#vatMode').off('change.mgf').on('change.mgf', function () {
            (w.recalcTotals || function () { })('#manuel_grid');
        });
    }

    var __recalc = w.recalcTotals;
    w.recalcTotals = function (sel) {
        ensureVatModeToolbar();
        if (typeof __recalc === 'function') return __recalc(sel);
    };

    $(function () { ensureVatModeToolbar(); });

})(jQuery, window, document);
/* ==== END: Manuel Grid Finalizer ==== */
