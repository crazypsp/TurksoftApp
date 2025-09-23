import { KuponService } from '../entities/KuponService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-kupon-body');
  const btnNew = $('#btnNewKupon');
  const modalEl = $('#mdlKupon'); 
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKupon');
  const fmId = $('#frmId');
  
  async function loadTable() {
    const list = await KuponService.list();
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
      const r = await KuponService.get(id);
      fmId.value = r.Id;
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KuponService.remove(id); 
      await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined };
    if (dto.Id) await KuponService.update(dto.Id, dto);
    else await KuponService.create(dto);
    modal?.hide(); 
    await loadTable();
  });

  await loadTable();
});
