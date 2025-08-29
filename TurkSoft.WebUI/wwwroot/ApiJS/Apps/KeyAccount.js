import { KeyAccountApi } from '../entities/index.js';
const $ = s => document.querySelector(s); const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', () => {
  const tbody = $('#tbl-ka-body');
  const fKod = $('#fltKod'); const fAcik = $('#fltAciklama');

  const btnNew = $('#btnNewKA');
  const modalEl = $('#mdlKA'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKA');

  const fmId = $('#frmId'), fmKod = $('#frmKod'), fmAcik = $('#frmAciklama');

  async function loadTable() {
    const list = await KeyAccountApi.list();
    const qK = val(fKod).toLowerCase(); const qA = val(fAcik).toLowerCase();
    const filtered = (list || []).filter(x => {
      const okK = !qK || (x.Kod || '').toLowerCase().includes(qK);
      const okA = !qA || (x.Aciklama || '').toLowerCase().includes(qA);
      return okK && okA;
    });
    tbody.innerHTML = (filtered || []).map(r => `
      <tr>
        <td>${r.Kod || ''}</td>
        <td>${r.Aciklama || ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await KeyAccountApi.get(id);
      fmId.value = r.Id; fmKod.value = r.Kod || ''; fmAcik.value = r.Aciklama || '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KeyAccountApi.remove(id); await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined, Kod: fmKod.value, Aciklama: fmAcik.value };
    if (dto.Id) await KeyAccountApi.update(dto.Id, dto); else await KeyAccountApi.create(dto);
    modal?.hide(); await loadTable();
  });

  [fKod, fAcik].forEach(el => el?.addEventListener('input', loadTable));
  loadTable();
});
