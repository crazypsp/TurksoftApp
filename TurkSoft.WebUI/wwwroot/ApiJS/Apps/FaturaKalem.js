import { FaturaKalemApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-faturakalem-body');
  const btnNew = $('#btnNewFaturaKalem');
  const modalEl = $('#mdlFaturaKalem');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmFaturaKalem');

  // Form alanları
  const fmId = $('#frmId');
  const fmFaturaId = $('#frmFaturaId');
  const fmPaketId = $('#frmPaketId');
  const fmMiktar = $('#frmMiktar');
  const fmBirimFiyat = $('#frmBirimFiyat');
  const fmTutar = $('#frmTutar');

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
    const list = await FaturaKalemApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.FaturaId ?? ''}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.Miktar ?? ''}</td>
        <td>${r.BirimFiyat ?? ''}</td>
        <td>${r.Tutar ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await FaturaKalemApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmFaturaId.value = r.FaturaId ?? '';
      fmPaketId.value = r.PaketId ?? '';
      fmMiktar.value = r.Miktar ?? '';
      fmBirimFiyat.value = r.BirimFiyat ?? '';
      fmTutar.value = r.Tutar ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await FaturaKalemApi.remove(id);
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
      FaturaId: val(fmFaturaId),
      PaketId: val(fmPaketId),
      Miktar: parseInt(val(fmMiktar)) || 0,
      BirimFiyat: parseFloat(val(fmBirimFiyat)) || 0,
      Tutar: parseFloat(val(fmTutar)) || 0
    };

    if (dto.Id) {
      await FaturaKalemApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await FaturaKalemApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
