import { EntegrasyonHesabiApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-entegrasyonhesabi-body');
  const btnNew = $('#btnNewEntegrasyonHesabi');
  const modalEl = $('#mdlEntegrasyonHesabi');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmEntegrasyonHesabi');

  // Form alanları
  const fmId = $('#frmId');
  const fmSistemTipi = $('#frmSistemTipi');
  const fmHost = $('#frmHost');
  const fmVeritabaniAdi = $('#frmVeritabaniAdi');
  const fmKullaniciAdi = $('#frmKullaniciAdi');
  const fmParola = $('#frmParola');
  const fmApiUrl = $('#frmApiUrl');
  const fmApiKey = $('#frmApiKey');
  const fmMaliMusavirId = $('#frmMaliMusavirId');
  const fmFirmaId = $('#frmFirmaId');
  const fmAciklama = $('#frmAciklama');

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
    const list = await EntegrasyonHesabiApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.SistemTipi ?? ''}</td>
        <td>${r.Host ?? ''}</td>
        <td>${r.VeritabaniAdi ?? ''}</td>
        <td>${r.ApiUrl ?? ''}</td>
        <td>${r.MaliMusavirId ?? ''}</td>
        <td>${r.FirmaId ?? ''}</td>
        <td>${r.Aciklama ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await EntegrasyonHesabiApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmSistemTipi.value = r.SistemTipi ?? '';
      fmHost.value = r.Host ?? '';
      fmVeritabaniAdi.value = r.VeritabaniAdi ?? '';
      fmKullaniciAdi.value = r.KullaniciAdi ?? '';
      fmParola.value = r.Parola ?? '';
      fmApiUrl.value = r.ApiUrl ?? '';
      fmApiKey.value = r.ApiKey ?? '';
      fmMaliMusavirId.value = r.MaliMusavirId ?? '';
      fmFirmaId.value = r.FirmaId ?? '';
      fmAciklama.value = r.Aciklama ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await EntegrasyonHesabiApi.remove(id);
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
      SistemTipi: val(fmSistemTipi),
      Host: val(fmHost),
      VeritabaniAdi: val(fmVeritabaniAdi),
      KullaniciAdi: val(fmKullaniciAdi),
      Parola: val(fmParola),
      ApiUrl: val(fmApiUrl),
      ApiKey: val(fmApiKey),
      MaliMusavirId: val(fmMaliMusavirId) || null,
      FirmaId: val(fmFirmaId) || null,
      Aciklama: val(fmAciklama)
    };

    if (dto.Id) {
      await EntegrasyonHesabiApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await EntegrasyonHesabiApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
