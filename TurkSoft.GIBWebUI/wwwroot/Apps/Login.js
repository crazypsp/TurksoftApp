(function () {
    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email);
    }
    function setInvalid($input, message) {
        $input.addClass('is-invalid');
        var $fb = $input.siblings('.invalid-feedback');
        if ($fb.length === 0) $fb = $('<div class="invalid-feedback"></div>').insertAfter($input);
        $fb.text(message);
    }
    function clearInvalid($input) { $input.removeClass('is-invalid'); }

    $('#loginForm').on('submit', function (e) {
        e.preventDefault(); // normal POST'u durdur
        var $form = $(this);
        var homeUrl = $form.data('home-url'); // Razor tarafından render edilen URL
        var $emailInput = $('#Email');
        var $passInput = $('#Password');
        var email = $emailInput.val().trim();
        var pass = $passInput.val().trim();
        var ok = true;

        if (!email) { setInvalid($emailInput, 'E-posta zorunludur.'); ok = false; }
        else if (!isValidEmail(email)) { setInvalid($emailInput, 'Geçerli bir e-posta adresi girin.'); ok = false; }
        else { clearInvalid($emailInput); }

        if (!pass) { setInvalid($passInput, 'Şifre zorunludur.'); ok = false; }
        else { clearInvalid($passInput); }

        if (ok) {
            // Tüm kontroller geçtiyse Home/Index'e yönlendir
            window.location.href = homeUrl;
        }
    });

    $('#Email, #Password').on('input', function () { $(this).removeClass('is-invalid'); });
})();