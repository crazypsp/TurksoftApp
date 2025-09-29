import { AdresApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-adres-body');
  const btnNew = $('#btnNewAdres');
  const modalEl = $('#mdlAdres');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmAdres');

  // Form alanları
  const fmId = $('#frmId');
  const fmUlke = $('#frmUlke');
  const fmSehir = $('#frmSehir');
  const fmIlce = $('#frmIlce');
  const fmPostaKodu = $('#frmPostaKodu');
  const fmAcikAdres = $('#frmAcikAdres');

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
    const list = await AdresApi.list();
    await logIslem("List"); // Listeleme logu
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Ulke ?? ''}</td>
        <td>${r.Sehir ?? ''}</td>
        <td>${r.Ilce ?? ''}</td>
        <td>${r.PostaKodu ?? ''}</td>
        <td>${r.AcikAdres ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await AdresApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmUlke.value = r.Ulke ?? '';
      fmSehir.value = r.Sehir ?? '';
      fmIlce.value = r.Ilce ?? '';
      fmPostaKodu.value = r.PostaKodu ?? '';
      fmAcikAdres.value = r.AcikAdres ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await AdresApi.remove(id);
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
      Ulke: val(fmUlke),
      Sehir: val(fmSehir),
      Ilce: val(fmIlce),
      PostaKodu: val(fmPostaKodu),
      AcikAdres: val(fmAcikAdres)
    };

    if (dto.Id) {
      await AdresApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await AdresApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
