import { PaketIskontoApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-paketiskonto-body');
  const btnNew = $('#btnNewPaketIskonto');
  const modalEl = $('#mdlPaketIskonto');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmPaketIskonto');

  // Form alanları
  const fmId = $('#frmId');
  const fmPaketId = $('#frmPaketId');
  const fmBayiId = $('#frmBayiId');
  const fmIskontoYuzde = $('#frmIskontoYuzde');
  const fmBaslangic = $('#frmBaslangic');
  const fmBitis = $('#frmBitis');

  // ---- Log helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log yazılamadı:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await PaketIskontoApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.IskontoYuzde ?? ''}</td>
        <td>${r.Baslangic ? new Date(r.Baslangic).toLocaleDateString() : ''}</td>
        <td>${r.Bitis ? new Date(r.Bitis).toLocaleDateString() : ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await PaketIskontoApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmPaketId.value = r.PaketId ?? '';
        fmBayiId.value = r.BayiId ?? '';
        fmIskontoYuzde.value = r.IskontoYuzde ?? '';
        fmBaslangic.value = r.Baslangic ? new Date(r.Baslangic).toISOString().substring(0, 10) : '';
        fmBitis.value = r.Bitis ? new Date(r.Bitis).toISOString().substring(0, 10) : '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await PaketIskontoApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      PaketId: val(fmPaketId),
      BayiId: val(fmBayiId) || null,
      IskontoYuzde: parseFloat(val(fmIskontoYuzde)) || 0,
      Baslangic: fmBaslangic.value ? new Date(fmBaslangic.value).toISOString() : null,
      Bitis: fmBitis.value ? new Date(fmBitis.value).toISOString() : null
    };

    if (dto.Id) {
      await PaketIskontoApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await PaketIskontoApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
