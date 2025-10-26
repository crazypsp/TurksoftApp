// wwwroot/js/pages/EArchive/CreateNewEarchiveInvoice.js
import * as EArchiveApi from '../Entites/EArchiveInvoice.js';

// Yardımcı: "label -> input" eşleşmesi (view'e dokunmadan alan yakalama)
function fieldByLabel($root, labelText) {
    // .form-group içindeki label metnini bul, kardeş input/select/textarea döndür
    const $lbl = $root.find('label').filter(function () {
        const t = ($(this).text() || '').trim().toLowerCase();
        return t.startsWith(labelText.toLowerCase());
    }).first();
    if ($lbl.length === 0) return $();
    const $grp = $lbl.closest('.form-group, .form-group.row');
    // grup içinde ilk input/select/textarea'yı al
    const $ctl = $grp.find('input,select,textarea').first();
    return $ctl;
}

// Yardımcı: tarih/saat stringlerini ISO'ya çevir (boşsa null)
function toISODate(v) {
    if (!v) return null;
    // <input type="date"> → YYYY-MM-DD
    if (/^\d{4}-\d{2}-\d{2}$/.test(v)) return new Date(v + 'T00:00:00').toISOString();
    // dd.MM.yyyy gibi başka formatlar gelirse:
    const m = v.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})$/);
    if (m) return new Date(+m[3], +m[2] - 1, +m[1]).toISOString();
    const d = new Date(v);
    return isNaN(d) ? null : d.toISOString();
}
function valNum(v) {
    if (v == null) return null;
    const n = parseFloat(('' + v).replace(/\./g, '').replace(',', '.'));
    return isNaN(n) ? null : n;
}

$(document).ready(function () {
    const $form = $('#faturaformu');

    // Üst butonları metnine göre yakala (ID yok; view'e dokunmuyoruz)
    const $btnPdf = $form.find('.box-header .btn').filter((i, el) => $(el).text().trim().startsWith('Fatura PDF'));
    const $btnXml = $form.find('.box-header .btn').filter((i, el) => $(el).text().trim().startsWith('Fatura XML'));
    const $btnPreview = $form.find('.box-header .btn').filter((i, el) => $(el).text().trim().startsWith('Fatura Önizle'));
    const $btnDraft = $form.find('.box-header .btn').filter((i, el) => $(el).text().trim().startsWith('Taslak Kaydet'));
    const $btnSend = $form.find('.box-header .btn').filter((i, el) => $(el).text().trim().startsWith("Gib'e Gönder"));

    // === JSON derleme (view alanları değişmeden) ===
    function collectDto() {
        const $topBox = $form.find('.box').eq(0);        // üst bilgiler box
        const $tabsBox = $form.find('.box').eq(1);        // tablar box

        // --- Üst Bilgiler (Sol/Orta/Sağ kolonlar) ---
        const header = {
            branch: fieldByLabel($topBox, 'Şube').val() || null,
            ettn: fieldByLabel($topBox, 'ETTN').val() || null,
            prefix: fieldByLabel($topBox, 'Fatura Ön Eki').val() || null,
            type: fieldByLabel($topBox, 'Fatura Tipi').val() || null,
            issueDate: toISODate(fieldByLabel($topBox, 'Fatura Tarihi').val()),
            issueTime: fieldByLabel($topBox, 'Fatura Saati').val() || null,
            orderNumber: fieldByLabel($topBox, 'Sipariş No').val() || null,
            orderDate: toISODate(fieldByLabel($topBox, 'Sipariş Tarihi').val()),
            currency: fieldByLabel($topBox, 'Para Birimi').val() || null
        };

        // --- Sekmeler ---
        const $tabAlici = $tabsBox.find('#alicibilgileri');
        const $tabIrs = $tabsBox.find('#irsaliye');
        const $tabEk = $tabsBox.find('#ekalan');
        const $tabOdeme = $tabsBox.find('#odeme');

        const buyer = {
            taxNo: fieldByLabel($tabAlici, 'VKN/TCKN').val() || null,
            alias: fieldByLabel($tabAlici, 'Alıcı Etiketi').val() || null,
            companyName: fieldByLabel($tabAlici, 'Firma Adı').val() || null,
            firstName: fieldByLabel($tabAlici, 'Ad').val() || null,
            lastName: fieldByLabel($tabAlici, 'Soyad').val() || null,
            email: ($tabAlici.find('input[type="email"]').val() || null),

            country: fieldByLabel($tabAlici, 'Ülke').val() || null,
            city: fieldByLabel($tabAlici, 'İl ').val() || fieldByLabel($tabAlici, 'İl *').val() || null,
            district: fieldByLabel($tabAlici, 'İlçe').val() || null,
            taxOffice: fieldByLabel($tabAlici, 'Vergi Dairesi').val() || null,
            address: ($tabAlici.find('textarea').first().val() || null),
            phone: fieldByLabel($tabAlici, 'Telefon').val() || null,
            postalCode: fieldByLabel($tabAlici, 'Posta Kodu').val() || null
        };

        const dispatch = {
            waybillNo: fieldByLabel($tabIrs, 'İrsaliye No').val() || null,
            waybillDate: toISODate(fieldByLabel($tabIrs, 'İrsaliye Tarihi').val()),
            carrierTitle: fieldByLabel($tabIrs, 'Taşıyıcı Ünvan').val() || null,
            carrierTaxNo: fieldByLabel($tabIrs, 'Taşıyıcı VKN/TCKN').val() || null,
            shipFrom: fieldByLabel($tabIrs, 'Sevk Yeri').val() || null,
            deliveryDate: toISODate(fieldByLabel($tabIrs, 'Teslimat Tarihi').val())
        };

        const extras = {
            branchCode: fieldByLabel($tabEk, 'Şube Kodu').val() || null,
            extra1: fieldByLabel($tabEk, 'Ek Bilgi 1').val() || null,
            extra2: fieldByLabel($tabEk, 'Ek Bilgi 2').val() || null,
            note: ($tabEk.find('textarea').first().val() || null),
            // Dosya yükleme: sadece adı alıyoruz (gerçek yükleme backend tarafında)
            attachmentName: $tabEk.find('input[type="file"]').prop('files')?.[0]?.name || null
        };

        const payment = {
            method: fieldByLabel($tabOdeme, 'Ödeme Yöntemi').val() || null,
            dueDate: toISODate(fieldByLabel($tabOdeme, 'Vade Tarihi').val()),
            amount: valNum(fieldByLabel($tabOdeme, 'Tutar').val()),
            bankName: fieldByLabel($tabOdeme, 'Banka Adı').val() || null,
            iban: fieldByLabel($tabOdeme, 'IBAN').val() || null,
            description: ($tabOdeme.find('textarea').first().val() || null)
        };

        // e-Arşiv faturalarında satır/kalem bu view'de yok;
        // gerekirse backend satırları farklı ekrandan alır.
        return {
            type: 'EARCHIVE',
            header, buyer, dispatch, extras, payment
        };
    }

    // === İşlemler ===
    async function saveDraft() {
        const dto = collectDto();
        dto.status = 'Draft';
        const res = await EArchiveApi.create(dto);
        toastr.success('Taslak kaydedildi.');
        return res; // { id, ... }
    }

    async function sendToGib() {
        const dto = collectDto();
        dto.status = 'ReadyToSend';
        const res = await EArchiveApi.create(dto);
        toastr.success('GİB\'e gönderme kuyruğuna alındı.');
        return res;
    }

    async function doPreview() {
        const dto = collectDto();
        if (EArchiveApi.preview) {
            // Backend 'preview' endpoint'i varsa: PDF/HTML blob dönebilir
            const resp = await EArchiveApi.preview(dto);
            if (resp?.blob) {
                const url = URL.createObjectURL(resp.blob);
                window.open(url, '_blank');
                return;
            }
        }
        // Alternatif: hızlı HTML önizleme
        const w = window.open('', '_blank');
        w.document.write(`<pre style="white-space:pre-wrap;word-break:break-word">${JSON.stringify(dto, null, 2)}</pre>`);
        w.document.close();
    }

    // === Buton bağlama ===
    $btnDraft.off('click').on('click', async (e) => {
        e.preventDefault();
        try { await saveDraft(); } catch (err) { console.error(err); toastr.error(err.message || 'Kaydedilemedi.'); }
    });

    $btnSend.off('click').on('click', async (e) => {
        e.preventDefault();
        try { await sendToGib(); } catch (err) { console.error(err); toastr.error(err.message || 'Gönderilemedi.'); }
    });

    $btnPreview.off('click').on('click', async (e) => {
        e.preventDefault();
        try { await doPreview(); } catch (err) { console.error(err); toastr.error('Önizleme hatası.'); }
    });

    // PDF/XML: önce taslak kaydedip dönen id ile (opsiyonel)
    $btnPdf.off('click').on('click', async (e) => {
        e.preventDefault();
        try {
            const res = await saveDraft();
            if (EArchiveApi.exportPdf && res?.id) {
                const r = await EArchiveApi.exportPdf(res.id);
                if (r?.blob) {
                    const url = URL.createObjectURL(r.blob);
                    window.open(url, '_blank');
                } else {
                    toastr.info('PDF indirme uç noktası blob döndürmedi.');
                }
            } else {
                toastr.info('PDF servisi tanımlı değil; backend action gerekli.');
            }
        } catch (err) { console.error(err); toastr.error('PDF indirilemedi.'); }
    });

    $btnXml.off('click').on('click', async (e) => {
        e.preventDefault();
        try {
            const res = await saveDraft();
            if (EArchiveApi.exportXml && res?.id) {
                const r = await EArchiveApi.exportXml(res.id);
                if (r?.blob) {
                    const url = URL.createObjectURL(r.blob);
                    window.open(url, '_blank');
                } else {
                    toastr.info('XML indirme uç noktası blob döndürmedi.');
                }
            } else {
                toastr.info('XML servisi tanımlı değil; backend action gerekli.');
            }
        } catch (err) { console.error(err); toastr.error('XML indirilemedi.'); }
    });

    // sayfa açılış logu
    console.log('e-Arşiv Yeni Fatura sayfa scripti hazır.');
});
