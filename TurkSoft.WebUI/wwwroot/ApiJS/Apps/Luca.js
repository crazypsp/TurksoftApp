import { LucaApi } from '../entities/index.js';
const $ = s => document.querySelector(s); const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', () => {
  const tbody = $('#tbl-luca-body');
  const fUye = $('#fltUyeNo'); const fUser = $('#fltKullaniciAdi');

  const btnNew = $('#btnNewLuca');
  const modalEl = $('#mdlLuca'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLuca');

  const fmId = $('#frmId'), fmUye = $('#frmUyeNo'), fmUser = $('#frmKullaniciAdi'), fmPwd = $('#frmParola');

  async function loadTable() {
    const list = await LucaApi.list();
    const qU = val(fUye).toLowerCase(); const qK = val(fUser).toLowerCase();
    const filtered = (list || []).filter(x => {
      const okU = !qU || (x.UyeNo || '').toLowerCase().includes(qU);
      const okK = !qK || (x.KullaniciAdi || '').toLowerCase().includes(qK);
      return okU && okK;
    });
    tbody.innerHTML = (filtered || []).map(r => `
      <tr>
        <td>${r.UyeNo || ''}</td>
        <td>${r.KullaniciAdi || ''}</td>
        <td>••••••••</td>
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
      const r = await LucaApi.get(id);
      fmId.value = r.Id; fmUye.value = r.UyeNo || ''; fmUser.value = r.KullaniciAdi || ''; fmPwd.value = r.Parola || '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LucaApi.remove(id); await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = { Id: fmId.value || undefined, UyeNo: fmUye.value, KullaniciAdi: fmUser.value, Parola: fmPwd.value };
    if (dto.Id) await LucaApi.update(dto.Id, dto); else await LucaApi.create(dto);
    modal?.hide(); await loadTable();
  });

  [fUye, fUser].forEach(el => el?.addEventListener('input', loadTable));
  loadTable();
});
