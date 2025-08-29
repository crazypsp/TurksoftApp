import { BayiApi } from '../entities/index.js';
const $ = s => document.querySelector(s); const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', () => {
  const tbody = $('#tbl-bayi-body');
  const fKod = $('#fltKod');
  const fUnv = $('#fltUnvan');

  const btnNew = $('#btnNewBayi');
  const modalEl = $('#mdlBayi'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmBayi');

  const fmId = $('#frmId'); const fmKod = $('#frmKod'); const fmUnv = $('#frmUnvan');
  const fmTel = $('#frmTelefon'); const fmEpo = $('#frmEposta');

  async function loadTable() {
    const list = await BayiApi.list();
    const k = val(fKod).toLowerCase(); const u = val(fUnv).toLowerCase();
    const filtered = (list || []).filter(x => {
      const okK = !k || (x.Kod || '').toLowerCase().includes(k);
      const okU = !u || (x.Unvan || '').toLowerCase().includes(u);
      return okK && okU;
    });
    tbody.innerHTML = (filtered || []).map(r => `
      <tr>
        <td>${r.Kod || ''}</td>
        <td>${r.Unvan || ''}</td>
        <td>${r.Telefon || ''}</td>
        <td>${r.Eposta || ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await BayiApi.get(id);
      fmId.value = r.Id; fmKod.value = r.Kod || ''; fmUnv.value = r.Unvan || '';
      fmTel.value = r.Telefon || ''; fmEpo.value = r.Eposta || '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await BayiApi.remove(id); await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined, Kod: fmKod.value, Unvan: fmUnv.value, Telefon: fmTel.value, Eposta: fmEpo.value };
    if (dto.Id) await BayiApi.update(dto.Id, dto); else await BayiApi.create(dto);
    modal?.hide(); await loadTable();
  });

  [fKod, fUnv].forEach(el => { el?.addEventListener('input', loadTable); });
  loadTable();
});
