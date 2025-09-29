import { SatisApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-satis-body');
  const btnNew = $('#btnNewSatis');
  const modalEl = $('#mdlSatis');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSatis');

  // Form alanları
  const fmId = $('#frmId');
  const fmSatisNo = $('#frmSatisNo');
  const fmSatisTarihi = $('#frmSatisTarihi');
  const fmBayiId = $('#frmBayiId');
  const fmMaliMusavirId = $('#frmMaliMusavirId');
  const fmFirmaId = $('#frmFirmaId');
  const fmPaketId = $('#frmPaketId');
  const fmKDVOrani = $('#frmKDVOrani');
  const fmKDVTutar = $('#frmKDVTutar');
  const fmIskontoTutar = $('#frmIskontoTutar');
  const fmToplamTutar = $('#frmToplamTutar');
  const fmNetTutar = $('#frmNetTutar');
  const fmSatisDurumu = $('#frmSatisDurumu');

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
        Islem: `${userId}-Satis-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydedilemedi:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await SatisApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.SatisNo ?? ''}</td>
        <td>${r.SatisTarihi ? new Date(r.SatisTarihi).toLocaleDateString() : ''}</td>
        <td>${r.BayiId ?? ''}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.ToplamTutar ?? ''}</td>
        <td>${r.NetTutar ?? ''}</td>
        <td>${r.SatisDurumu ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SatisApi.get(id);
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
        fmSatisNo.value = r.SatisNo ?? '';
        fmSatisTarihi.value = r.SatisTarihi ? r.SatisTarihi.substring(0, 10) : '';
        fmBayiId.value = r.BayiId ?? '';
        fmMaliMusavirId.value = r.MaliMusavirId ?? '';
        fmFirmaId.value = r.FirmaId ?? '';
        fmPaketId.value = r.PaketId ?? '';
        fmKDVOrani.value = r.KDVOrani ?? '';
        fmKDVTutar.value = r.KDVTutar ?? '';
        fmIskontoTutar.value = r.IskontoTutar ?? '';
        fmToplamTutar.value = r.ToplamTutar ?? '';
        fmNetTutar.value = r.NetTutar ?? '';
        fmSatisDurumu.value = r.SatisDurumu ?? '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SatisApi.remove(id);
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

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      SatisNo: val(fmSatisNo),
      SatisTarihi: val(fmSatisTarihi),
      BayiId: val(fmBayiId),
      MaliMusavirId: val(fmMaliMusavirId),
      FirmaId: val(fmFirmaId) || null,
      PaketId: val(fmPaketId),
      KDVOrani: parseFloat(val(fmKDVOrani)) || 0,
      KDVTutar: parseFloat(val(fmKDVTutar)) || 0,
      IskontoTutar: parseFloat(val(fmIskontoTutar)) || 0,
      ToplamTutar: parseFloat(val(fmToplamTutar)) || 0,
      NetTutar: parseFloat(val(fmNetTutar)) || 0,
      SatisDurumu: parseInt(val(fmSatisDurumu)) || 0
    };

    if (dto.Id) {
      await SatisApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SatisApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
