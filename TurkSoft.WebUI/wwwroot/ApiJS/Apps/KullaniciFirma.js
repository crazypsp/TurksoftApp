import { KullaniciFirmaService } from '../entities/KullaniciFirmaService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-kullanicifirma-body');
  const btnNew = $('#btnNewKullaniciFirma');
  const modalEl = $('#mdlKullaniciFirma'); 
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKullaniciFirma');
  const fmId = $('#frmId');
  
  async function loadTable() {
    const list = await KullaniciFirmaService.list();
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
      const r = await KullaniciFirmaService.get(id);
      fmId.value = r.Id;
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KullaniciFirmaService.remove(id); 
      await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined };
    if (dto.Id) await KullaniciFirmaService.update(dto.Id, dto);
    else await KullaniciFirmaService.create(dto);
    modal?.hide(); 
    await loadTable();
  });

  await loadTable();
});
