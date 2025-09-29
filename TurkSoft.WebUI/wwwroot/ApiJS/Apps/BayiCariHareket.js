import { BayiCariHareketApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-bayicarihareket-body');
  const btnNew = $('#btnNewBayiCariHareket');
  const modalEl = $('#mdlBayiCariHareket');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmBayiCariHareket');

  // Form alanları
  const fmId = $('#frmId');
  const fmBayiCariId = $('#frmBayiCariId');
  const fmIslemTarihi = $('#frmIslemTarihi');
  const fmTip = $('#frmTip');
  const fmTutar = $('#frmTutar');
  const fmAciklama = $('#frmAciklama');
  const fmReferansId = $('#frmReferansId');
  const fmReferansTip = $('#frmReferansTip');

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
    const list = await BayiCariHareketApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.IslemTarihi ?? ''}</td>
        <td>${r.Tip ?? ''}</td>
        <td>${r.Tutar ?? ''}</td>
        <td>${r.Aciklama ?? ''}</td>
        <td>${r.ReferansTip ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await BayiCariHareketApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmBayiCariId.value = r.BayiCariId ?? '';
      fmIslemTarihi.value = r.IslemTarihi ?? '';
      fmTip.value = r.Tip ?? '';
      fmTutar.value = r.Tutar ?? '';
      fmAciklama.value = r.Aciklama ?? '';
      fmReferansId.value = r.ReferansId ?? '';
      fmReferansTip.value = r.ReferansTip ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await BayiCariHareketApi.remove(id);
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
      BayiCariId: val(fmBayiCariId),
      IslemTarihi: val(fmIslemTarihi),
      Tip: val(fmTip),
      Tutar: parseFloat(val(fmTutar)) || 0,
      Aciklama: val(fmAciklama),
      ReferansId: val(fmReferansId),
      ReferansTip: val(fmReferansTip)
    };

    if (dto.Id) {
      await BayiCariHareketApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await BayiCariHareketApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
