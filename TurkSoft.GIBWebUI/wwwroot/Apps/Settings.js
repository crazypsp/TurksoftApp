// wwwroot/js/pages/Settings/Settings.js
/* Basit yardımcılar */
(function () {
    window.showWarning = function (msg) {
        $("#lblModalUyari").text(msg || "Uyarı");
        $("#modal-uyari").modal("show");
    };

    window.LucaModal = function () {
        $("#modal-LucaLogin").modal("show");
    };

    window.PrefixCodeEdit = function (id, subeId) {
        // demo doldurma
        $("#subeler").val(String(subeId));
        $("#NumaraTurIdType").val("1");
        $("#OnEk").val("NEW");
        $("#Yil").val(new Date().getFullYear());
        $("#Sayi").val("0");
        $("#aktifPasif").prop("checked", true);
        $("#kulaniciBazli").prop("checked", false);
        $(".prefixUser").addClass("hidden");
        $("#prefixcode_modal").modal("show");
    };

    window.PrefixCodeDelete = function (id, subeId) {
        if (confirm("Kaydı silmek istiyor musunuz?")) {
            if (window.toastr) toastr.success("Silindi (demo).");
        }
    };

    window.TanimliAlanKaydet = function (el) {
        if (window.toastr) toastr.info("Ayar güncellendi.");
    };
})();


/* Sayfa init ve event kablolama */
$(function () {
    // Select2 init (varsa)
    if ($.fn.select2) {
        $(".select2").select2({ width: "100%" });
    }

    /* ==== MODAL AÇAN BUTONLAR ==== */
    $("#btnManuelXslt").on("click", function (e) {
        e.preventDefault();
        $("#xsltYukle").modal("show");
    });

    $("#btnYeniXslt").on("click", function (e) {
        e.preventDefault();
        if (window.toastr) toastr.info("Yeni XSLT tasarım akışı (demo).");
    });

    $("#btnXsltManuelKaydet").on("click", function () {
        if (window.toastr) toastr.success("XSLT kaydedildi (demo).");
        $("#xsltYukle").modal("hide");
    });

    $("#btnXsltOnizle").on("click", function () {
        if (window.toastr) toastr.info("Önizleme hazırlanıyor (demo).");
    });

    $("#btnXsltKaydet").on("click", function () {
        if (window.toastr) toastr.success("XSLT kaydedildi (demo).");
    });

    $("#btnBankEkle").on("click", function () {
        // form temizle
        $("#bankaForm").trigger("reset");
        $("#banking_modal").modal("show");
    });

    $("#btnBankKaydet").on("click", function () {
        if (window.toastr) toastr.success("Banka kaydedildi (demo).");
        $("#banking_modal").modal("hide");
    });

    $("#PrefixOlustur").on("click", function () {
        $("#PrefixCodeKayit").trigger("reset");
        $("#aktifPasif").prop("checked", false);
        $("#kulaniciBazli").prop("checked", false);
        $(".prefixUser").addClass("hidden");
        $("#prefixcode_modal").modal("show");
    });

    $("#kulaniciBazli").on("change", function () {
        if ($(this).is(":checked")) $(".prefixUser").removeClass("hidden");
        else $(".prefixUser").addClass("hidden");
    });

    $("#PrefixCodeSave").on("click", function () {
        if (window.toastr) toastr.success("Belge ön ek kaydedildi (demo).");
        $("#prefixcode_modal").modal("hide");
    });

    $("#btnAramaYap").on("click", function () {
        if (window.toastr) toastr.info("Arama yapıldı (demo).");
    });

    $("#btnTemizle").on("click", function () {
        $("#SubeAdi,#BelgeOnEki,#filterYil,#SonBelgeSayisi").val("");
        $("#BelgeTur").val("");
        if (window.toastr) toastr.info("Filtreler temizlendi.");
    });

    // Kullanıcı & Rol modalları
    $("#btnRolEkle, #btnYeniRolEkle").on("click", function () {
        $("#modal-rolEkle").modal("show");
    });

    $("#roklebtn").on("click", function () {
        if (window.toastr) toastr.success("Rol kaydedildi (demo).");
    });

    $("#btnUserAddMdl").on("click", function () {
        $("#modal-UserEkle").modal("show");
    });

    $("#userAddBtn").on("click", function () {
        if (window.toastr) toastr.success("Kullanıcı kaydedildi (demo).");
        $("#modal-UserEkle").modal("hide");
    });

    // E-posta ayarları
    $("#emailAyarKaydetBtn").on("click", function () {
        if (window.toastr) toastr.success("E-posta ayarları kaydedildi (demo).");
    });
    $("#emailAyarSilBtn").on("click", function () {
        if (window.toastr) toastr.info("E-posta ayarları silindi (demo).");
    });
    $("#btnTestMailGonder").on("click", function () {
        if (window.toastr) toastr.info("Test mail gönderildi (demo).");
    });

    // Bildirim kuralları
    $("#btnNotRuleEkle").on("click", function () {
        if (window.toastr) toastr.success("Bildirim kuralı eklendi (demo).");
    });

    // Kullanıcı bazlı gelen ekle
    $("#btnInboxAdd").on("click", function () {
        if (window.toastr) toastr.success("Kayıt eklendi (demo).");
    });

    // Parametreler
    $("#btnDegerEkle").on("click", function () {
        if (window.toastr) toastr.info("Değer eklendi (demo).");
    });
    $("#btnParametreEkle").on("click", function () {
        if (window.toastr) toastr.success("Parametre eklendi (demo).");
    });

    // Cihaz Ayarları
    $("#chkSelectAll").on("change", function () {
        const chk = $(this).is(":checked");
        $("#alertDeviceGrid tbody input[type=checkbox]").prop("checked", chk);
    });
    $("#btnBulkDelete").on("click", function (e) {
        e.preventDefault();
        if (window.toastr) toastr.info("Seçili cihazlar silindi (demo).");
    });

    // Veritabanı test/kaydet
    $("#veriTabaniTest").on("click", function () {
        if (window.toastr) toastr.info("Bağlantı testi başarılı (demo).");
    });
    $("#veriTabaniKaydet").on("click", function () {
        if (window.toastr) toastr.success("Ayarlar kaydedildi (demo).");
    });

    // Luca Login akışı (dummy)
    $("#lucaLoginControl").on("click", function () {
        $("#LucaUzunAdKontrol").prop("disabled", false);
        if (window.toastr) toastr.success("Luca giriş kontrol ok (demo).");
    });
    $("#LucaUzunAdKontrol").on("click", function () {
        $("#lucaGMKayit").prop("disabled", false);
        if (window.toastr) toastr.info("Uzun ad kontrol edildi (demo).");
    });
    $("#lucaGMKayit").on("click", function () {
        if (window.toastr) toastr.success("Luca bilgiler kaydedildi (demo).");
        $("#modal-LucaLogin").modal("hide");
    });
});
