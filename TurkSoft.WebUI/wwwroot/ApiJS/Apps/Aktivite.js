import { AktiviteApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-aktivite-body');
  const btnNew = $('#btnNewAktivite');
  const modalEl = $('#mdlAktivite');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmAktivite');

  // Form alanları
  const fmId = $('#frmId');
  const fmKonu = $('#frmKonu');
  const fmTur = $('#frmTur');
  const fmDurum = $('#frmDurum');
  const fmPlanlananTarih = $('#frmPlanlananTarih');
  const fmGerceklesenTarih = $('#frmGerceklesenTarih');
  const fmIlgiliId = $('#frmIlgiliId');
  const fmIlgiliTip = $('#frmIlgiliTip');
  const fmIlgiliKullaniciId = $('#frmIlgiliKullaniciId');
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
    const list = await AktiviteApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Konu ?? ''}</td>
        <td>${r.Tur ?? ''}</td>
        <td>${r.Durum ?? ''}</td>
        <td>${r.PlanlananTarih ?? ''}</td>
        <td>${r.GerceklesenTarih ?? ''}</td>
        <td>${r.IlgiliTip ?? ''}</td>
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
    const r = await AktiviteApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmKonu.value = r.Konu ?? '';
      fmTur.value = r.Tur ?? '';
      fmDurum.value = r.Durum ?? '';
      fmPlanlananTarih.value = r.PlanlananTarih ?? '';
      fmGerceklesenTarih.value = r.GerceklesenTarih ?? '';
      fmIlgiliId.value = r.IlgiliId ?? '';
      fmIlgiliTip.value = r.IlgiliTip ?? '';
      fmIlgiliKullaniciId.value = r.IlgiliKullaniciId ?? '';
      fmAciklama.value = r.Aciklama ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await AktiviteApi.remove(id);
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
      Konu: val(fmKonu),
      Tur: val(fmTur),
      Durum: val(fmDurum),
      PlanlananTarih: val(fmPlanlananTarih),
      GerceklesenTarih: val(fmGerceklesenTarih),
      IlgiliId: val(fmIlgiliId),
      IlgiliTip: val(fmIlgiliTip),
      IlgiliKullaniciId: val(fmIlgiliKullaniciId),
      Aciklama: val(fmAciklama)
    };

    if (dto.Id) {
      await AktiviteApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await AktiviteApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
