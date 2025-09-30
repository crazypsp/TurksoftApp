import { SmsAyarApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-smsayar-body');
  const btnNew = $('#btnNewSmsAyar');
  const modalEl = $('#mdlSmsAyar');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSmsAyar');

  // Form alanları
  const fmId = $('#frmId');
  const fmApiKey = $('#frmApiKey');
  const fmApiSecret = $('#frmApiSecret');
  const fmGondericiAdi = $('#frmGondericiAdi');

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
        Islem: `${userId}-SmsAyar-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydı başarısız:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await SmsAyarApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.ApiKey ?? ''}</td>
        <td>${r.GondericiAdi ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SmsAyarApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmApiKey.value = r.ApiKey ?? '';
        fmApiSecret.value = r.ApiSecret ?? '';
        fmGondericiAdi.value = r.GondericiAdi ?? '';

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SmsAyarApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert / Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    const dto = {
      Id: fmId.value || undefined,
      ApiKey: val(fmApiKey),
      ApiSecret: val(fmApiSecret),
      GondericiAdi: val(fmGondericiAdi)
    };

    if (dto.Id) {
      await SmsAyarApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SmsAyarApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
