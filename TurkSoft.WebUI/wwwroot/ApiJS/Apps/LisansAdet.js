import { LisansAdetService } from '../entities/LisansAdetService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-lisansadet-body');
  const btnNew = $('#btnNewLisansAdet');
  const modalEl = $('#mdlLisansAdet'); 
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLisansAdet');
  const fmId = $('#frmId');
  
  async function loadTable() {
    const list = await LisansAdetService.list();
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${Object.values(r)[1] ?? ''}</td>
        <td>${Object.values(r)[2] ?? ''}</td>
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
      const r = await LisansAdetService.get(id);
      fmId.value = r.Id;
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LisansAdetService.remove(id); 
      await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined };
    if (dto.Id) await LisansAdetService.update(dto.Id, dto);
    else await LisansAdetService.create(dto);
    modal?.hide(); 
    await loadTable();
  });

  await loadTable();
});
