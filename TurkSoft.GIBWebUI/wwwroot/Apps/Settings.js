// wwwroot/js/pages/Settings/Settings.js
import { SettingsApi } from '../Entities/index.js';
const $$ = sel => $(sel).toArray();

$(document).ready(function () {
    const fmCompany = $('#CompanyName');
    const fmVkn = $('#VknTckn');
    const fmAddress = $('#Address');
    const fmFullName = $('#FullName');
    const fmEmail = $('#Email');
    const fmPhone = $('#Phone');
    const fmTheme = $('#Theme');
    const fmLanguage = $('#Language');

    async function loadSettings() {
        try {
            const data = await SettingsApi.list();
            if (!data || data.length === 0) return;
            const s = data[0];
            fmCompany.val(s.companyName || '');
            fmVkn.val(s.vknTckn || '');
            fmAddress.val(s.address || '');
            fmFullName.val(s.fullName || '');
            fmEmail.val(s.email || '');
            fmPhone.val(s.phone || '');
            fmTheme.val(s.theme || 'red');
            fmLanguage.val(s.language || 'tr');
        } catch (err) {
            console.error('Ayarlar yüklenemedi:', err);
            toastr.error('Ayarlar yüklenemedi.');
        }
    }

    async function saveSettings() {
        const dto = {
            companyName: fmCompany.val(),
            vknTckn: fmVkn.val(),
            address: fmAddress.val(),
            fullName: fmFullName.val(),
            email: fmEmail.val(),
            phone: fmPhone.val(),
            theme: fmTheme.val(),
            language: fmLanguage.val()
        };

        try {
            const all = await SettingsApi.list();
            if (all && all.length > 0)
                await SettingsApi.update(all[0].id, dto);
            else
                await SettingsApi.create(dto);

            toastr.success('Ayarlar başarıyla kaydedildi.');
        } catch (err) {
            console.error('Kaydetme hatası:', err);
            toastr.error('Ayarlar kaydedilemedi.');
        }
    }

    // 💾 Enter tuşu veya alan değişiminde kaydet
    $('input, textarea, select').on('change', function () {
        saveSettings();
    });

    loadSettings();
});
