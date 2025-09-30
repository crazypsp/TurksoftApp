import { LisansApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-lisans-body');
  const btnNew = $('#btnNewLisans');
  const modalEl = $('#mdlLisans');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLisans');

  // Form alanları
  const fmId = $('#frmId');
  const fmLisansAnahtari = $('#frmLisansAnahtari');
  const fmBaslangicTarihi = $('#frmBaslangicTarihi');
  const fmBitisTarihi = $('#frmBitisTarihi');
  const fmYenilendiMi = $('#frmYenilendiMi');
  const fmSatisId = $('#frmSatisId');

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
    const list = await LisansApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.LisansAnahtari ?? ''}</td>
        <td>${r.BaslangicTarihi ?? ''}</td>
        <td>${r.BitisTarihi ?? ''}</td>
        <td>${r.YenilendiMi ? 'Evet' : 'Hayır'}</td>
        <td>${r.SatisId ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await LisansApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmLisansAnahtari.value = r.LisansAnahtari ?? '';
      fmBaslangicTarihi.value = r.BaslangicTarihi ?? '';
      fmBitisTarihi.value = r.BitisTarihi ?? '';
      fmYenilendiMi.checked = r.YenilendiMi ?? false;
      fmSatisId.value = r.SatisId ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LisansApi.remove(id);
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
      LisansAnahtari: val(fmLisansAnahtari),
      BaslangicTarihi: val(fmBaslangicTarihi),
      BitisTarihi: val(fmBitisTarihi),
      YenilendiMi: fmYenilendiMi.checked,
      SatisId: val(fmSatisId)
    };

    if (dto.Id) {
      await LisansApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await LisansApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
