// Apps/Fatura.js
// =====================================================================
// e-Fatura Oluşturma / Güncelleme / Gönderim işlemleri
// Kullanici.js mimarisi baz alınmıştır.
// Veri tabanı tabloları: Invoices, InvoiceLines, Customers,
// InvoicePayments, InvoiceDespatch, InvoiceExtraFields
// API endpointleri: EFatura.* (SendUBLInvoice, UpdateUBLInvoice vb.)
// =====================================================================

import { getSession } from '../../Service/LoginService.js';
import { FaturaApi, InvoiceApi, InvoiceLineApi, CustomerApi } from '../entities/index.js';

// ------------------ yardımcılar ------------------
const $ = s => document.querySelector(s);
const val = el => (el?.value ?? '').trim();
const num = el => parseFloat(val(el).replace(',', '.')) || 0;
function toast(msg, type = 'info') { console.log(`[${type}] ${msg}`); }

// ------------------ oturum ------------------
let session = {};
let sessionUserId = null;

async function resolveSession() {
    try {
        const maybe = typeof getSession === 'function' ? getSession() : {};
        const s = (maybe && typeof maybe.then === 'function') ? await maybe : maybe;
        return s || {};
    } catch { return {}; }
}
function applySession(s) {
    session = s || {};
    sessionUserId = s?.UserId || s?.KullaniciId || s?.id || null;
}

// ------------------ fatura işlemleri ------------------
let currentInvoiceId = null;

// 1️⃣ Fatura verisini topla (tüm tablolar)
function collectInvoiceData() {
    // -- Invoice Header --
    const header = {
        InvoiceId: currentInvoiceId,
        UUID: val($('#txtETTN')),
        FaturaNo: val($('#txtFaturaNo')),
        FaturaTarihi: val($('#dtFaturaTarihi')),
        FaturaTipi: val($('#ddlFaturaTipi')),
        ParaBirimi: val($('#ddlParaBirimi')),
        Etiket: val($('#ddlEtiket')),
        Senaryo: val($('#ddlSenaryo')),
        Note: val($('#txtNot')),
        VKN: val($('#txtVkn')),
        Unvan: val($('#txtUnvan')),
        Adres: val($('#txtAdres')),
        ToplamTutar: num($('#tAra')),
        KdvTutari: num($('#tKdv20')),
        GenelToplam: num($('#tGenel')),
        OlusturanKullaniciId: sessionUserId
    };

    // -- Customer --
    const customer = {
        VKN: val($('#txtVkn')),
        Unvan: val($('#txtUnvan')),
        Ad: val($('#txtAd')),
        Soyad: val($('#txtSoyad')),
        Etiket: val($('#ddlAliciEtiketi')),
        VergiDairesi: val($('#txtVergiDairesi')),
        Il: val($('#txtIl')),
        Ilce: val($('#txtIlce')),
        Adres: val($('#txtAdres')),
        Email: val($('#txtEmail'))
    };

    // -- Despatch --
    const despatch = {
        IrsaliyeNo: val($('#txtIrsaliyeNo')),
        IrsaliyeTarihi: val($('#dtIrsaliyeTarihi')),
        SevkYeri: val($('#txtSevkYeri')),
        TeslimYeri: val($('#txtTeslimYeri'))
    };

    // -- Payment --
    const payment = {
        OdemeTipi: val($('#ddlOdemeTipi')),
        VadeTarihi: val($('#dtVadeTarihi')),
        Tutar: num($('#tGenel'))
    };

    // -- Extra Fields (SGK) --
    const extra = {
        ProvizyonNo: val($('#txtProvizyonNo')),
        TakipNo: val($('#txtTakipNo')),
        EkAlanJson: JSON.stringify({
            Notlar: val($('#txtNot')),
            Kur: num($('#txtKur'))
        })
    };

    // -- Lines (Satırlar) --
    const lines = [];
    $('#tblLines tbody tr').forEach?.call($('#tblLines tbody tr'), tr => {
        lines.push({
            MalHizmetAdi: val(tr.querySelector('.ln-ad')),
            Miktar: num(tr.querySelector('.ln-qty')),
            Birim: val(tr.querySelector('.ln-unit')),
            BirimFiyat: num(tr.querySelector('.ln-price')),
            Iskonto: num(tr.querySelector('.ln-discp')),
            KdvOrani: num(tr.querySelector('.ln-kdv')),
            KdvTutari: num(tr.querySelector('.ln-kdvt')),
            Tutar: num(tr.querySelector('.ln-net'))
        });
    });

    return { header, customer, despatch, payment, extra, lines };
}

// 2️⃣ Veritabanına kaydet
async function saveToDb() {
    const { header, customer, despatch, payment, extra, lines } = collectInvoiceData();

    try {
        // 1. müşteri kaydı
        const cust = await CustomerApi.createOrUpdate(customer);

        // 2. fatura kaydı
        const inv = await InvoiceApi.createOrUpdate({ ...header, CustomerId: cust.CustomerId });
        currentInvoiceId = inv.InvoiceId;

        // 3. ilişkili tablolar
        await InvoiceLineApi.bulkUpsert(inv.InvoiceId, lines);
        await InvoiceApi.saveDespatch(inv.InvoiceId, despatch);
        await InvoiceApi.savePayment(inv.InvoiceId, payment);
        await InvoiceApi.saveExtraFields(inv.InvoiceId, extra);

        toast('Fatura taslak olarak kaydedildi.', 'success');
        return inv;
    } catch (err) {
        console.error('Fatura kaydet hatası:', err);
        alert('Fatura kaydedilemedi.');
    }
}

// 3️⃣ XML kontrol ve gönderim
async function validateAndSend() {
    const inv = await saveToDb();

    // Ön kontrol
    try {
        await EFaturaApi.controlUBLXml(inv.UUID);
        toast('XML doğrulaması başarılı.', 'info');
    } catch {
        alert('XML doğrulaması başarısız.');
        return;
    }

    // Gönderim
    try {
        const inputDoc = [{
            documentUUID: inv.UUID,
            documentId: inv.FaturaNo,
            documentDate: inv.FaturaTarihi,
            localId: inv.InvoiceId,
            sourceUrn: inv.Etiket,
            destinationUrn: inv.EtiketAlici || '',
            documentNoPrefix: inv.FaturaNo?.substring(0, 3),
            note: inv.Note
        }];
        await EFaturaApi.sendUBLInvoice(inputDoc);
        toast('Fatura GİB’e başarıyla gönderildi.', 'success');
    } catch (err) {
        console.error('Gönderim hatası:', err);
        alert('Fatura gönderilemedi.');
    }
}

// 4️⃣ Fatura güncelleme
async function updateInvoice() {
    const inv = await saveToDb();
    try {
        const inputDoc = [{
            documentUUID: inv.UUID,
            documentId: inv.FaturaNo,
            localId: inv.InvoiceId,
            note: inv.Note
        }];
        await EFaturaApi.updateUBLInvoice(inputDoc);
        toast('Fatura güncellendi.', 'success');
    } catch (err) {
        console.error('Fatura güncelleme hatası:', err);
        alert('Fatura güncellenemedi.');
    }
}

// 5️⃣ Fatura iptal
async function cancelInvoice() {
    const uuid = val($('#txtETTN'));
    if (!uuid) { alert('UUID bulunamadı.'); return; }
    const reason = prompt('İptal nedeni:');
    const cancelDate = new Date().toISOString();
    try {
        await EFaturaApi.cancelUBLInvoice(uuid, reason, cancelDate);
        toast('Fatura iptal edildi.', 'warning');
    } catch (err) {
        console.error('İptal hatası:', err);
        alert('Fatura iptal edilemedi.');
    }
}

// 6️⃣ Kredi sorgulama
async function checkCredit() {
    const vkn = val($('#txtVkn'));
    if (!vkn) return alert('VKN giriniz');
    const count = await EFaturaApi.getCustomerCreditCount(vkn);
    toast(`Kalan kontör: ${count}`, 'info');
}

// ------------------ init ------------------
document.addEventListener('DOMContentLoaded', async () => {
    const s = await resolveSession();
    applySession(s);

    $('#btnSaveInvoice')?.addEventListener('click', saveToDb);
    $('#btnSendInvoice')?.addEventListener('click', validateAndSend);
    $('#btnUpdateInvoice')?.addEventListener('click', updateInvoice);
    $('#btnCancelInvoice')?.addEventListener('click', cancelInvoice);
    $('#btnCheckCredit')?.addEventListener('click', checkCredit);
});
