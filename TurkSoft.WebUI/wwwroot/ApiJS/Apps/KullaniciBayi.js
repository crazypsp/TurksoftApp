import { KullaniciBayiApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-kullanicibayi-body');
  const btnNew = $('#btnNewKullaniciBayi');
  const modalEl = $('#mdlKullaniciBayi');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKullaniciBayi');

  // Form alanları
  const fmId = $('#frmId');
  const fmKullaniciId = $('#frmKullaniciId');
  const fmBayiId = $('#frmBayiId');
  const fmIsPrimary = $('#frmIsPrimary');
  const fmAtananRol = $('#frmAtananRol');
  const fmBaslangicTarihi = $('#frmBaslangicTarihi');
  const fmBitisTarihi = $('#frmBitisTarihi');

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
    const list = await KullaniciBayiApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.KullaniciId ?? ''}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.IsPrimary ? 'Evet' : 'Hayır'}</td>
        <td>${r.AtananRol ?? ''}</td>
        <td>${r.BaslangicTarihi ?? ''}</td>
        <td>${r.BitisTarihi ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await KullaniciBayiApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmKullaniciId.value = r.KullaniciId ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmIsPrimary.checked = r.IsPrimary ?? false;
      fmAtananRol.value = r.AtananRol ?? '';
      fmBaslangicTarihi.value = r.BaslangicTarihi ?? '';
      fmBitisTarihi.value = r.BitisTarihi ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KullaniciBayiApi.remove(id);
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
      KullaniciId: val(fmKullaniciId),
      BayiId: val(fmBayiId),
      IsPrimary: fmIsPrimary.checked,
      AtananRol: val(fmAtananRol),
      BaslangicTarihi: val(fmBaslangicTarihi) || null,
      BitisTarihi: val(fmBitisTarihi) || null
    };

    if (dto.Id) {
      await KullaniciBayiApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await KullaniciBayiApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
