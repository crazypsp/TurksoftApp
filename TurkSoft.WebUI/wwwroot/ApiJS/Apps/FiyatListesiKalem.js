import { FiyatListesiKalemApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-fiyatlistesikalem-body');
  const btnNew = $('#btnNewFiyatListesiKalem');
  const modalEl = $('#mdlFiyatListesiKalem');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmFiyatListesiKalem');

  // Form alanları
  const fmId = $('#frmId');
  const fmFiyatListesiId = $('#frmFiyatListesiId');
  const fmPaketId = $('#frmPaketId');
  const fmBirimFiyat = $('#frmBirimFiyat');

  // ---- Log ekleme helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession(); // { Id, ... }
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
    const list = await FiyatListesiKalemApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.FiyatListesiId ?? ''}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.BirimFiyat ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await FiyatListesiKalemApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmFiyatListesiId.value = r.FiyatListesiId ?? '';
      fmPaketId.value = r.PaketId ?? '';
      fmBirimFiyat.value = r.BirimFiyat ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await FiyatListesiKalemApi.remove(id);
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
      FiyatListesiId: val(fmFiyatListesiId),
      PaketId: val(fmPaketId),
      BirimFiyat: parseFloat(val(fmBirimFiyat)) || 0
    };

    if (dto.Id) {
      await FiyatListesiKalemApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await FiyatListesiKalemApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
