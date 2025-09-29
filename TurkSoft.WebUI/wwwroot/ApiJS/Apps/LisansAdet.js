import { LisansAdetApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-lisansadet-body');
  const btnNew = $('#btnNewLisansAdet');
  const modalEl = $('#mdlLisansAdet');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLisansAdet');

  // Form alanları
  const fmId = $('#frmId');
  const fmLisansId = $('#frmLisansId');
  const fmKuruluCihazSayisi = $('#frmKuruluCihazSayisi');
  const fmLimit = $('#frmLimit');

  // ---- Log ekleme helper ----
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
    const list = await LisansAdetApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.LisansId ?? ''}</td>
        <td>${r.KuruluCihazSayisi ?? ''}</td>
        <td>${r.Limit ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await LisansAdetApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmLisansId.value = r.LisansId ?? '';
      fmKuruluCihazSayisi.value = r.KuruluCihazSayisi ?? '';
      fmLimit.value = r.Limit ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LisansAdetApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt butonu ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Kayıt ekleme / güncelleme ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      LisansId: val(fmLisansId),
      KuruluCihazSayisi: parseInt(val(fmKuruluCihazSayisi)) || 0,
      Limit: val(fmLimit) ? parseInt(val(fmLimit)) : null
    };

    if (dto.Id) {
      await LisansAdetApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await LisansAdetApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
