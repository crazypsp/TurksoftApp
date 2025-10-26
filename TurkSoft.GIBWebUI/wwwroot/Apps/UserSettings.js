// wwwroot/js/pages/Settings/UserSettings.js
import { UsersApi } from '../Entities/index.js';
const $$ = sel => $(sel).toArray();

$(document).ready(function () {
    const grid = $('#grid_users tbody');
    const fUsername = $('#fltUsername');
    const fEmail = $('#fltEmail');
    const btnFilter = $('#btnFilter');
    const btnNew = $('#btnNewUser');
    const modal = $('#mdlUser');
    const formEl = $('#frmUser');

    const fmId = $('#usrId');
    const fmUsername = $('#usrUsername');
    const fmEmail = $('#usrEmail');
    const fmPhone = $('#usrPhone');
    const fmIsActive = $('#usrIsActive');
    const fmIsSuper = $('#usrIsSuper');
    const fmPassword = $('#usrPassword');

    async function loadTable() {
        try {
            const data = await UsersApi.list();
            const usernameFilter = (fUsername.val() || '').toLowerCase();
            const emailFilter = (fEmail.val() || '').toLowerCase();

            const filtered = (data || []).filter(x =>
                (!usernameFilter || (x.username || '').toLowerCase().includes(usernameFilter)) &&
                (!emailFilter || (x.email || '').toLowerCase().includes(emailFilter))
            );

            const rows = (filtered || []).map(i => `
                <tr>
                    <td>${i.username || ''}</td>
                    <td>${i.email || ''}</td>
                    <td>${i.phone || ''}</td>
                    <td>${i.isActive ? '<span class="label label-success">Aktif</span>' : '<span class="label label-danger">Pasif</span>'}</td>
                    <td>${i.isSuperUser ? 'Evet' : 'Hayır'}</td>
                    <td>${i.lastLogin ? moment(i.lastLogin).format('DD.MM.YYYY HH:mm') : '-'}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-primary act-edit" data-id="${i.id}"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-sm btn-danger act-del" data-id="${i.id}"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join('');

            grid.html(rows);
            bindActions();
        } catch (err) {
            console.error('Kullanıcı listesi yüklenemedi:', err);
            toastr.error('Kullanıcı listesi yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).data('id');
            const u = await UsersApi.get(id);
            fmId.val(u.id);
            fmUsername.val(u.username);
            fmEmail.val(u.email);
            fmPhone.val(u.phone);
            fmIsActive.prop('checked', u.isActive);
            fmIsSuper.prop('checked', u.isSuperUser);
            fmPassword.val('');
            modal.modal('show');
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).data('id');
            if (!confirm('Bu kullanıcı silinsin mi?')) return;
            await UsersApi.remove(id);
            toastr.success('Kullanıcı silindi.');
            await loadTable();
        });
    }

    btnNew.on('click', function () {
        formEl[0].reset();
        fmId.val('');
        modal.modal('show');
    });

    formEl.on('submit', async function (e) {
        e.preventDefault();
        const dto = {
            id: fmId.val() || undefined,
            username: fmUsername.val(),
            email: fmEmail.val(),
            phone: fmPhone.val(),
            isActive: fmIsActive.is(':checked'),
            isSuperUser: fmIsSuper.is(':checked'),
            password: fmPassword.val() || undefined
        };

        try {
            if (dto.id) await UsersApi.update(dto.id, dto);
            else await UsersApi.create(dto);
            toastr.success('Kayıt başarıyla kaydedildi.');
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            toastr.error('Kayıt başarısız.');
            console.error(err);
        }
    });

    fUsername.on('input', loadTable);
    fEmail.on('input', loadTable);
    btnFilter.on('click', loadTable);

    loadTable();
});
