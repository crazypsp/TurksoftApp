// wwwroot/js/pages/Settings/CompanyInformations.js
import { CompanyInformationsApi, ContractApi, BranchApi } from '../Entities/index.js';
const $$ = sel => $(sel).toArray();

$(document).ready(function () {
    const formEl = $('#frmCompany');

    const fmId = $('#cmpId');
    const fmVkn = $('#vknTckn');
    const fmName = $('#musteriAdi');
    const fmTitle = $('#unvan');
    const fmKep = $('#kepAdresi');
    const fmRespTckn = $('#sorumluTckn');
    const fmRespAd = $('#sorumluAd');
    const fmRespSoyad = $('#sorumluSoyad');

    const gridContracts = $('#SozlesmeGrid tbody');
    const gridBranches = $('#SubeGrid tbody');

    async function loadCompany() {
        try {
            const data = await CompanyInformationsApi.list();
            if (!data || data.length === 0) return;
            const c = data[0];

            fmId.val(c.id);
            fmVkn.val(c.taxNo || '');
            fmName.val(c.companyName || '');
            fmTitle.val(c.title || '');
            fmKep.val(c.kepAddress || '');
            fmRespTckn.val(c.responsibleTckn || '');
            fmRespAd.val(c.responsibleName || '');
            fmRespSoyad.val(c.responsibleSurname || '');

            await loadContracts(c.id);
            await loadBranches(c.id);
        } catch (err) {
            toastr.error('Firma bilgileri yüklenemedi.');
            console.error(err);
        }
    }

    async function loadContracts(companyId) {
        try {
            const data = await ContractApi.list(companyId);
            const rows = (data || []).map(x => `
                <tr>
                    <td>${x.startDate ? moment(x.startDate).format('DD.MM.YYYY') : ''} - ${x.endDate ? moment(x.endDate).format('DD.MM.YYYY') : ''}</td>
                    <td>${x.serviceName || ''}</td>
                    <td>${x.tariffType || ''}</td>
                    <td>${x.status || ''}</td>
                    <td><button class="btn btn-sm btn-primary"><i class="fa fa-eye"></i></button></td>
                </tr>`).join('');
            gridContracts.html(rows);
        } catch (err) {
            console.error('Sözleşmeler yüklenemedi:', err);
            gridContracts.html('<tr><td colspan="5">Veri alınamadı</td></tr>');
        }
    }

    async function loadBranches(companyId) {
        try {
            const data = await BranchApi.list(companyId);
            const rows = (data || []).map(x => `
                <tr>
                    <td>${x.type || ''}</td>
                    <td>${x.code || ''}</td>
                    <td>${x.name || ''}</td>
                    <td>${x.city || ''}</td>
                    <td>${x.district || ''}</td>
                    <td>${x.phone || ''}</td>
                    <td>${x.email || ''}</td>
                    <td><button class="btn btn-sm btn-primary"><i class="fa fa-edit"></i></button></td>
                </tr>`).join('');
            gridBranches.html(rows);
        } catch (err) {
            console.error('Şubeler yüklenemedi:', err);
            gridBranches.html('<tr><td colspan="8">Veri alınamadı</td></tr>');
        }
    }

    formEl.on('submit', async function (e) {
        e.preventDefault();
        const dto = {
            id: fmId.val() || undefined,
            taxNo: fmVkn.val(),
            companyName: fmName.val(),
            title: fmTitle.val(),
            kepAddress: fmKep.val(),
            responsibleTckn: fmRespTckn.val(),
            responsibleName: fmRespAd.val(),
            responsibleSurname: fmRespSoyad.val()
        };

        try {
            if (dto.id) await CompanyInformationsApi.update(dto.id, dto);
            else await CompanyInformationsApi.create(dto);
            toastr.success('Firma bilgileri güncellendi.');
            await loadCompany();
        } catch (err) {
            toastr.error('Kaydetme hatası.');
            console.error(err);
        }
    });

    loadCompany();
});
