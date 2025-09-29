import { MailGonderimApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-mailgonderim-body');
  const btnNew = $('#btnNewMailGonderim');
  const modalEl = $('#mdlMailGonderim');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmMailGonderim');

  // Form alanları
  const fmId = $('#frmId');
  const fmAlici = $('#frmAlici');
  const fmKonu = $('#frmKonu');
  const fmIcerik = $('#frmIcerik');
  const fmBasariliMi = $('#frmBasariliMi');

  // ---- Log helper ----
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
    const list = await MailGonderimApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Alici ?? ''}</td>
        <td>${r.Konu ?? ''}</td>
        <td>${r.BasariliMi ? 'Başarılı' : 'Başarısız'}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await MailGonderimApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmAlici.value = r.Alici ?? '';
      fmKonu.value = r.Konu ?? '';
      fmIcerik.value = r.Icerik ?? '';
      fmBasariliMi.checked = r.BasariliMi ?? false;

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await MailGonderimApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      Alici: val(fmAlici),
      Konu: val(fmKonu),
      Icerik: val(fmIcerik),
      BasariliMi: fmBasariliMi.checked
    };

    if (dto.Id) {
      await MailGonderimApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await MailGonderimApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
