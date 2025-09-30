import { KullaniciMaliMusavirApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-kullanicimalimusavir-body');
  const btnNew = $('#btnNewKullaniciMaliMusavir');
  const modalEl = $('#mdlKullaniciMaliMusavir');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKullaniciMaliMusavir');

  // Form alanları
  const fmId = $('#frmId');
  const fmKullaniciId = $('#frmKullaniciId');
  const fmMaliMusavirId = $('#frmMaliMusavirId');
  const fmIsPrimary = $('#frmIsPrimary');
  const fmAtananRol = $('#frmAtananRol');
  const fmBaslangicTarihi = $('#frmBaslangicTarihi');
  const fmBitisTarihi = $('#frmBitisTarihi');

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
    const list = await KullaniciMaliMusavirApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.KullaniciId ?? ''}</td>
        <td>${r.MaliMusavirId ?? ''}</td>
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
    const r = await KullaniciMaliMusavirApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmKullaniciId.value = r.KullaniciId ?? '';
      fmMaliMusavirId.value = r.MaliMusavirId ?? '';
      fmIsPrimary.checked = r.IsPrimary ?? false;
      fmAtananRol.value = r.AtananRol ?? '';
      fmBaslangicTarihi.value = r.BaslangicTarihi ?? '';
      fmBitisTarihi.value = r.BitisTarihi ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KullaniciMaliMusavirApi.remove(id);
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
      MaliMusavirId: val(fmMaliMusavirId),
      IsPrimary: fmIsPrimary.checked,
      AtananRol: val(fmAtananRol),
      BaslangicTarihi: val(fmBaslangicTarihi) || null,
      BitisTarihi: val(fmBitisTarihi) || null
    };

    if (dto.Id) {
      await KullaniciMaliMusavirApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await KullaniciMaliMusavirApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
