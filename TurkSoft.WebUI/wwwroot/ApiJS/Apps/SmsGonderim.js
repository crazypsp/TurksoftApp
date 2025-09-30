import { SmsGonderimApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-smsgonderim-body');
  const btnNew = $('#btnNewSmsGonderim');
  const modalEl = $('#mdlSmsGonderim');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSmsGonderim');

  // Form alanları
  const fmId = $('#frmId');
  const fmAliciNumara = $('#frmAliciNumara');
  const fmMesaj = $('#frmMesaj');
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
        Islem: `${userId}-SmsGonderim-${tip}`,
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
    const list = await SmsGonderimApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.AliciNumara ?? ''}</td>
        <td>${r.Mesaj?.substring(0, 30) ?? ''}...</td>
        <td>${r.BasariliMi ? "Başarılı" : "Başarısız"}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SmsGonderimApi.get(id);
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
        fmAliciNumara.value = r.AliciNumara ?? '';
        fmMesaj.value = r.Mesaj ?? '';
        fmBasariliMi.checked = r.BasariliMi ?? false;

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SmsGonderimApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    fmBasariliMi.checked = false;
    modal?.show();
  });

  // ---- Insert / Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    const dto = {
      Id: fmId.value || undefined,
      AliciNumara: val(fmAliciNumara),
      Mesaj: val(fmMesaj),
      BasariliMi: fmBasariliMi.checked
    };

    if (dto.Id) {
      await SmsGonderimApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SmsGonderimApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
