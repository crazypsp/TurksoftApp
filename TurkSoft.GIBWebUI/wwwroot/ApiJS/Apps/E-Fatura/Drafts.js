// Apps/Drafts.js
// =====================================================================
// Taslak e-Faturalar (Drafts) yönetimi
// Fatura.js mimarisiyle birebir
// Tablolar: Invoices, Customers, InvoiceLines, InvoiceStatusLogs
// API: EFaturaDraftApi, InvoiceApi, InvoiceLineApi, CustomerApi, InvoiceStatusLogApi
// =====================================================================

import { getSession } from '../Service/LoginService.js';
import { DraftsApi, InvoiceApi, InvoiceLineApi, CustomerApi, InvoiceStatusLogApi } from '../entities/index.js';

// --------------------- Yardımcılar ---------------------
const $ = s => document.querySelector(s);
const val = el => (el?.value ?? '').trim();
function toast(msg, type = 'info') { console.log(`[${type.toUpperCase()}] ${msg}`); }
function getCheckedIds() {
    return [...document.querySelectorAll('#draftInvoiceTable tbody input[type=checkbox]:checked')]
        .map(cb => cb.closest('tr').dataset.id);
}

// --------------------- Oturum ---------------------
let session = {};
let sessionUserId = null;

async function resolveSession() {
    try {
        const s = await (typeof getSession === 'function' ? getSession() : {});
        return s || {};
    } catch { return {}; }
}
function applySession(s) {
    session = s || {};
    sessionUserId = s?.UserId || s?.KullaniciId || s?.id || null;
}

// --------------------- Taslak Listeleme ---------------------
let dt = null;
const tbody = $('#draftInvoiceTable tbody');

function renderTable(items) {
    if (!tbody) return;
    tbody.innerHTML = (items || []).map(inv => `
    <tr data-id="${inv.InvoiceId}">
      <td><input type="checkbox" class="dt-checkboxes" /></td>
      <td>${inv.FaturaNo || ''}</td>
      <td>${inv.Unvan || ''}</td>
      <td>${inv.VKN || ''}</td>
      <td>${inv.FaturaTipi || ''} / ${inv.Senaryo || ''}</td>
      <td>${inv.FaturaTarihi ? new Date(inv.FaturaTarihi).toLocaleDateString('tr-TR') : ''}</td>
      <td>${Number(inv.GenelToplam || 0).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}</td>
      <td><span class="label label-success">Taslak</span></td>
      <td class="text-center">
        <button class="btn btn-info btn-sm btn-preview"><i class="fa fa-eye"></i></button>
        <button class="btn btn-warning btn-sm btn-edit"><i class="fa fa-edit"></i></button>
        <button class="btn btn-danger btn-sm btn-del"><i class="fa fa-trash"></i></button>
        <button class="btn btn-success btn-sm btn-send"><i class="fa fa-send"></i></button>
      </td>
    </tr>
  `).join('');

    if (window.DataTable) {
        if (dt) dt.destroy();
        dt = new window.DataTable('#draftInvoiceTable', {
            language: { url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Turkish.json' },
            responsive: true,
            searching: false,
            lengthChange: false
        });
    }
}

async function loadDrafts() {
    const list = await EFaturaDraftApi.queryUBLInvoice('documentStatus', 'Draft');
    renderTable(list);
    toast(`Toplam ${list?.length || 0} taslak fatura bulundu.`, 'info');
}

// --------------------- Filtreleme ---------------------
function filterLocal() {
    const vkn = val($('#TargetIdentifier')).toLowerCase();
    const title = val($('#TargetTitle')).toLowerCase();
    const start = val($('#IssueDateStart'));
    const end = val($('#IssueDateStartEnd'));

    const rows = [...tbody.querySelectorAll('tr')];
    rows.forEach(tr => {
        const tVkn = tr.children[3]?.innerText.toLowerCase() || '';
        const tTitle = tr.children[2]?.innerText.toLowerCase() || '';
        const tDate = tr.children[5]?.innerText || '';
        const date = tDate ? new Date(tDate.split('.').reverse().join('-')) : null;

        const show = (!vkn || tVkn.includes(vkn))
            && (!title || tTitle.includes(title))
            && (!start || (date && date >= new Date(start)))
            && (!end || (date && date <= new Date(end)));

        tr.style.display = show ? '' : 'none';
    });
}

function clearSearchInputs() {
    ['IssueDateStart', 'IssueDateStartEnd', 'TargetIdentifier', 'TargetTitle']
        .forEach(id => { const el = $('#' + id); if (el) el.value = ''; });
    filterLocal();
}

// --------------------- İşlemler ---------------------
async function logStatus(invoiceId, status, description = '') {
    await InvoiceStatusLogApi.create({
        InvoiceId: invoiceId,
        Status: status,
        Description: description,
        ChangedBy: sessionUserId,
        ChangedDate: new Date().toISOString()
    });
}

async function deleteDraft(id) {
    if (!confirm('Taslak fatura silinsin mi?')) return;
    await InvoiceApi.remove(id);
    await logStatus(id, 'Silindi', 'Taslak fatura kullanıcı tarafından silindi.');
    toast('Taslak fatura silindi.', 'warning');
    await loadDrafts();
}

async function sendDraft(id) {
    const inv = await InvoiceApi.get(id);
    if (!inv) return alert('Fatura bulunamadı.');

    const customer = await CustomerApi.get(inv.CustomerId);
    const lines = await InvoiceLineApi.listByInvoice(inv.InvoiceId);

    const inputDoc = [{
        documentUUID: inv.UUID,
        documentId: inv.FaturaNo,
        documentDate: inv.FaturaTarihi,
        localId: inv.InvoiceId,
        sourceUrn: inv.Etiket,
        destinationUrn: customer?.Etiket,
        note: inv.Note
    }];

    try {
        await EFaturaDraftApi.sendUBLInvoice(inputDoc);
        await logStatus(inv.InvoiceId, 'Gönderildi', 'Taslak fatura GİB\'e gönderildi.');
        toast(`Fatura ${inv.FaturaNo} GİB'e gönderildi.`, 'success');
    } catch (err) {
        await logStatus(inv.InvoiceId, 'Hata', `Gönderim hatası: ${err.message}`);
        alert('Gönderim sırasında hata oluştu.');
    }

    await loadDrafts();
}

async function previewDraft(id) {
    const inv = await InvoiceApi.get(id);
    const xml = inv?.XmlContent || '';
    await EFaturaDraftApi.controlUBLXml(xml);
    await logStatus(inv.InvoiceId, 'Önizlendi', 'XML kontrolü yapıldı.');
    alert(`Fatura ${inv.FaturaNo} XML kontrolünden geçti.`);
}

async function checkCredit(vkn) {
    const result = await EFaturaDraftApi.getCustomerCreditCount(vkn);
    alert(`Kalan e-Fatura kontör sayısı: ${result}`);
}

// --------------------- Toplu İşlemler ---------------------
async function sendAllDraft() {
    const ids = getCheckedIds();
    if (!ids.length) return alert('Hiç seçim yapılmadı.');
    for (const id of ids) await sendDraft(id);
}

async function deleteAllDraft() {
    const ids = getCheckedIds();
    if (!ids.length) return alert('Hiç seçim yapılmadı.');
    if (!confirm(`${ids.length} taslak silinsin mi?`)) return;
    for (const id of ids) {
        await InvoiceApi.remove(id);
        await logStatus(id, 'Silindi', 'Toplu silme işlemi.');
    }
    toast(`${ids.length} taslak silindi.`, 'warning');
    await loadDrafts();
}

function createNewDraft() {
    window.location.href = '/Fatura/Create';
}

// --------------------- Eventler ---------------------
document.addEventListener('DOMContentLoaded', async () => {
    const s = await resolveSession();
    applySession(s);
    await loadDrafts();

    $('#draftInvoiceTable')?.addEventListener('click', async e => {
        const tr = e.target.closest('tr[data-id]');
        if (!tr) return;
        const id = tr.dataset.id;
        if (e.target.closest('.btn-del')) await deleteDraft(id);
        else if (e.target.closest('.btn-send')) await sendDraft(id);
        else if (e.target.closest('.btn-preview')) await previewDraft(id);
        else if (e.target.closest('.btn-edit')) window.location.href = `/Fatura/Edit/${id}`;
    });

    window.gridSearch = filterLocal;
    window.clearSearchInputs = clearSearchInputs;
    window.createNewDraft = createNewDraft;
    window.sendAllDraft = sendAllDraft;
    window.deleteAllDraft = deleteAllDraft;
});
