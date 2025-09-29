import { FiyatListesiApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-fiyatlistesi-body');
  const btnNew = $('#btnNewFiyatListesi');
  const modalEl = $('#mdlFiyatListesi');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmFiyatListesi');

  // Form alanları
  const fmId = $('#frmId');
  const fmKod = $('#frmKod');
  const fmAd = $('#frmAd');
  const fmBayiId = $('#frmBayiId');
  const fmBaslangic = $('#frmBaslangic');
  const fmBitis = $('#frmBitis');

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
    const list = await FiyatListesiApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Kod ?? ''}</td>
        <td>${r.Ad ?? ''}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.Baslangic ?? ''}</td>
        <td>${r.Bitis ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await FiyatListesiApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmKod.value = r.Kod ?? '';
      fmAd.value = r.Ad ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmBaslangic.value = r.Baslangic ?? '';
      fmBitis.value = r.Bitis ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await FiyatListesiApi.remove(id);
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
      Kod: val(fmKod),
      Ad: val(fmAd),
      BayiId: val(fmBayiId) || null,
      Baslangic: val(fmBaslangic),
      Bitis: val(fmBitis) || null
    };

    if (dto.Id) {
      await FiyatListesiApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await FiyatListesiApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
