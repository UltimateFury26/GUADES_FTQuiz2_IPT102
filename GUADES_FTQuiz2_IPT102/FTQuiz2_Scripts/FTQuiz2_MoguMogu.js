// Mogu Mogu shell behavior (Phase 1). Loaded after FTQuiz2_Portfolio.js in the same bundle.
// Handles only the new header: mobile menu, Admin dropdown, one-menu-at-a-time, scroll shadow.
document.addEventListener("DOMContentLoaded", function () {

    var header = document.getElementById("mmHeader");
    var nav = document.getElementById("mmNav");
    var navToggle = document.getElementById("mmNavToggle");
    var adminMenu = document.getElementById("adminMenu");
    var accountMenu = document.getElementById("accountMenu");

    // ---- Mobile menu ----
    function setNav(open) {
        if (!nav || !navToggle) { return; }
        nav.classList.toggle("open", open);
        navToggle.setAttribute("aria-expanded", open ? "true" : "false");
        navToggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
    }

    if (nav && navToggle) {
        navToggle.addEventListener("click", function () {
            setNav(!nav.classList.contains("open"));
        });

        // Choosing a link or the Contact button closes the mobile menu.
        nav.querySelectorAll("a, button").forEach(function (control) {
            control.addEventListener("click", function () { setNav(false); });
        });

        document.addEventListener("click", function (e) {
            if (header && !header.contains(e.target)) { setNav(false); }
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 900) { setNav(false); }
        });
    }

    // ---- Admin dropdown: close on outside click / Esc; never open together with the account menu ----
    if (adminMenu) {
        document.addEventListener("click", function (e) {
            if (!adminMenu.contains(e.target)) { adminMenu.removeAttribute("open"); }
        });

        adminMenu.addEventListener("toggle", function () {
            if (adminMenu.open && accountMenu) { accountMenu.removeAttribute("open"); }
        });
    }

    if (accountMenu) {
        accountMenu.addEventListener("toggle", function () {
            if (accountMenu.open && adminMenu) { adminMenu.removeAttribute("open"); }
        });
    }

    document.addEventListener("keydown", function (e) {
        if (e.key !== "Escape") { return; }
        if (adminMenu) { adminMenu.removeAttribute("open"); }
        setNav(false);
    });

    // ---- Stronger header shadow once the page scrolls ----
    function onScroll() {
        if (header) { header.classList.toggle("is-scrolled", window.scrollY > 4); }
    }
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();
});
