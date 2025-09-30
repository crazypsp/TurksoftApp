import { FaturaApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-fatura-body');
  const btnNew = $('#btnNewFatura');
  const modalEl = $('#mdlFatura');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmFatura');

  // Form alanları
  const fmId = $('#frmId');
  const fmFaturaNo = $('#frmFaturaNo');
  const fmBayiId = $('#frmBayiId');
  const fmSatisId = $('#frmSatisId');
  const fmFirmaId = $('#frmFirmaId');
  const fmTip = $('#frmTip');
  const fmDurum = $('#frmDurum');
  const fmFaturaTarihi = $('#frmFaturaTarihi');
  const fmKdvoran = $('#frmKdvoran');
  const fmKdvtutar = $('#frmKdvtutar');
  const fmToplam = $('#frmToplam');
  const fmNet = $('#frmNet');

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
    const list = await FaturaApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.FaturaNo ?? ''}</td>
        <td>${r.FaturaTarihi ?? ''}</td>
        <td>${r.Toplam ?? ''}</td>
        <td>${r.Net ?? ''}</td>
        <td>${r.Durum ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await FaturaApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmFaturaNo.value = r.FaturaNo ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmSatisId.value = r.SatisId ?? '';
      fmFirmaId.value = r.FirmaId ?? '';
      fmTip.value = r.Tip ?? '';
      fmDurum.value = r.Durum ?? '';
      fmFaturaTarihi.value = r.FaturaTarihi ?? '';
      fmKdvoran.value = r.Kdvoran ?? '';
      fmKdvtutar.value = r.Kdvtutar ?? '';
      fmToplam.value = r.Toplam ?? '';
      fmNet.value = r.Net ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await FaturaApi.remove(id);
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
      FaturaNo: val(fmFaturaNo),
      BayiId: val(fmBayiId),
      SatisId: val(fmSatisId) || null,
      FirmaId: val(fmFirmaId),
      Tip: val(fmTip),
      Durum: val(fmDurum),
      FaturaTarihi: val(fmFaturaTarihi),
      Kdvoran: parseFloat(val(fmKdvoran)) || 0,
      Kdvtutar: parseFloat(val(fmKdvtutar)) || 0,
      Toplam: parseFloat(val(fmToplam)) || 0,
      Net: parseFloat(val(fmNet)) || 0
    };

    if (dto.Id) {
      await FaturaApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await FaturaApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
