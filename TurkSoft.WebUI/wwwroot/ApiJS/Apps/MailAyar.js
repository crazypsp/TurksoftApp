import { MailAyarApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-mailayar-body');
  const btnNew = $('#btnNewMailAyar');
  const modalEl = $('#mdlMailAyar');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmMailAyar');

  // Form alanları
  const fmId = $('#frmId');
  const fmSmtpServer = $('#frmSmtpServer');
  const fmPort = $('#frmPort');
  const fmEposta = $('#frmEposta');
  const fmSifre = $('#frmSifre');
  const fmSSLKullan = $('#frmSSLKullan');

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
    const list = await MailAyarApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.SmtpServer ?? ''}</td>
        <td>${r.Eposta ?? ''}</td>
        <td>${r.Port ?? ''}</td>
        <td>${r.SSLKullan ? 'Evet' : 'Hayır'}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await MailAyarApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmSmtpServer.value = r.SmtpServer ?? '';
      fmPort.value = r.Port ?? '';
      fmEposta.value = r.Eposta ?? '';
      fmSifre.value = r.Sifre ?? '';
      fmSSLKullan.checked = r.SSLKullan ?? false;

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await MailAyarApi.remove(id);
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
      SmtpServer: val(fmSmtpServer),
      Port: parseInt(val(fmPort)) || 0,
      Eposta: val(fmEposta),
      Sifre: val(fmSifre),
      SSLKullan: fmSSLKullan.checked
    };

    if (dto.Id) {
      await MailAyarApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await MailAyarApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
