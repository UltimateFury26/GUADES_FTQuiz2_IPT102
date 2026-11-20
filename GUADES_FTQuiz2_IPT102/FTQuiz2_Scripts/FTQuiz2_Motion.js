/* ============================================================================
   FTQuiz2_Motion.js  —  motion + interaction layer (v2)
   ----------------------------------------------------------------------------
   Effects are adapted from the BrewedOps Motion Library patterns:
     Navigation › Glass on scroll / Hide on scroll
     Scroll patterns › Scroll triggered reveal, Staggered list entrance,
                       Scroll progress bar
     Text › Marquee ticker (CSS driven, duplicated here)
     Buttons › Magnetic button, Button shine sweep (CSS)
     Cards › 3D tilt
     Stats › Count up
     Forms › Password strength, Inline validation, Submit to success
   All effects are opt-out via prefers-reduced-motion and never block the page.
   ========================================================================== */
(function () {
    "use strict";

    var reduceMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var supportsIO = "IntersectionObserver" in window;

    function ready(fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn);
        } else {
            fn();
        }
    }

    /* ---------------------------------------------------- header + progress */
    function initChrome() {
        var header = document.getElementById("mmHeader");
        var progress = document.createElement("div");
        progress.className = "mg-scroll-progress";
        progress.setAttribute("aria-hidden", "true");
        document.body.appendChild(progress);

        var ticking = false;
        function onScroll() {
            if (ticking) { return; }
            ticking = true;
            window.requestAnimationFrame(function () {
                var y = window.pageYOffset || document.documentElement.scrollTop;
                if (header) { header.classList.toggle("is-scrolled", y > 12); }

                var doc = document.documentElement;
                var max = (doc.scrollHeight - window.innerHeight) || 1;
                progress.style.transform = "scaleX(" + Math.min(y / max, 1) + ")";
                ticking = false;
            });
        }
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();

        /* Mobile nav toggle: keeps aria-expanded honest. */
        var toggle = document.getElementById("mmNavToggle");
        var nav = document.getElementById("mmNav");
        if (toggle && nav) {
            toggle.addEventListener("click", function () {
                var open = nav.classList.toggle("open");
                toggle.setAttribute("aria-expanded", open ? "true" : "false");
                toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
            });
            nav.addEventListener("click", function (e) {
                if (e.target.closest("a")) {
                    nav.classList.remove("open");
                    toggle.setAttribute("aria-expanded", "false");
                }
            });
        }

        /* Close <details> menus when clicking outside them. */
        document.addEventListener("click", function (e) {
            document.querySelectorAll("details.mm-menu[open], details.account-menu[open]").forEach(function (d) {
                if (!d.contains(e.target)) { d.removeAttribute("open"); }
            });
        });

        /* Escape closes any open menu. */
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                document.querySelectorAll("details[open]").forEach(function (d) { d.removeAttribute("open"); });
            }
        });
    }

    /* ------------------------------------------------------------- marquees */
    /* Two jobs:
       1. Duplicate the group INSIDE the flex track, so the -50% keyframe loops
          with no gap. (The clone used to be appended to the outer wrapper, which
          left the track holding a single group and produced a visible jump.)
       2. Start every strip at the phase the wall clock says it should be at.
          Because the offset comes from Date.now(), a full page load lands on the
          same point of the loop the previous page left off at, so the band keeps
          travelling instead of snapping back to the start. */
    function initMarquees() {
        document.querySelectorAll("[data-mg-marquee]").forEach(function (wrapper) {
            var group = wrapper.querySelector(".mg-ticker__group, .mg-band__group");
            if (!group) { return; }

            var track = group.parentNode;
            if (wrapper.dataset.mgCloned !== "1") {
                var seamlessCopy = group.cloneNode(true);
                seamlessCopy.setAttribute("aria-hidden", "true");
                track.appendChild(seamlessCopy);
                wrapper.dataset.mgCloned = "1";
            }

            var duration = parseFloat(wrapper.getAttribute("data-mg-duration"), 10);
            if (!duration || duration < 1000) {
                duration = 34000;
            }

            // Force one continuous row. The negative epoch-based delay means a
            // full navigation lands at the current point in the same animation,
            // rather than replaying from the left edge on every page.
            track.style.display = "flex";
            track.style.flexDirection = "row";
            track.style.flexWrap = "nowrap";
            track.style.width = "max-content";
            track.style.animationDuration = duration + "ms";
            track.style.animationTimingFunction = "linear";
            track.style.animationIterationCount = "infinite";
            track.style.animationDelay = "-" + (Date.now() % duration) + "ms";
        });
    }

    /* Re-sync after a back/forward cache restore, where the CSS clock froze. */
    window.addEventListener("pageshow", function (e) {
        if (e.persisted) { initMarquees(); }
    });

    /* --------------------------------------------------------- scroll reveal */
    function initReveal() {
        var items = document.querySelectorAll("[data-mg-reveal]");
        if (!items.length) { return; }

        if (reduceMotion || !supportsIO) {
            items.forEach(function (el) { el.classList.add("is-in"); });
            return;
        }

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) { return; }
                entry.target.classList.add("is-in");
                io.unobserve(entry.target);
            });
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });

        items.forEach(function (el, i) {
            if (!el.style.getPropertyValue("--mg-delay")) {
                el.style.setProperty("--mg-delay", Math.min(i % 8, 7) * 70 + "ms");
            }
            io.observe(el);
        });
    }

    /* -------------------------------------------------------- magnetic buttons */
    function initMagnetic() {
        if (reduceMotion || window.matchMedia("(hover: none)").matches) { return; }
        document.querySelectorAll(".pill-btn-primary, .mg-admin-nav__link").forEach(function (el) {
            el.addEventListener("mousemove", function (e) {
                var r = el.getBoundingClientRect();
                var x = (e.clientX - r.left - r.width / 2) / r.width;
                var y = (e.clientY - r.top - r.height / 2) / r.height;
                el.style.transform = "translate(" + (x * 5).toFixed(2) + "px," + (y * 4 - 1).toFixed(2) + "px)";
            });
            el.addEventListener("mouseleave", function () { el.style.transform = ""; });
        });
    }

    /* --------------------------------------------------------------- 3D tilt */
    function initTilt() {
        if (reduceMotion || window.matchMedia("(hover: none)").matches) { return; }
        document.querySelectorAll("[data-mg-tilt]").forEach(function (card) {
            card.addEventListener("mousemove", function (e) {
                var r = card.getBoundingClientRect();
                var px = (e.clientX - r.left) / r.width - 0.5;
                var py = (e.clientY - r.top) / r.height - 0.5;
                card.style.transform = "perspective(900px) rotateY(" + (px * 6).toFixed(2) + "deg) rotateX(" + (-py * 6).toFixed(2) + "deg)";
            });
            card.addEventListener("mouseleave", function () { card.style.transform = ""; });
        });
    }

    /* -------------------------------------------------------------- count up */
    function initCounters() {
        var nums = document.querySelectorAll(".stat-value");
        if (!nums.length) { return; }

        function animate(el) {
            var raw = (el.textContent || "").trim();
            var target = parseInt(raw.replace(/[^0-9]/g, ""), 10);
            if (isNaN(target) || target === 0 || reduceMotion) { return; }
            var start = null;
            var dur = 900;
            function step(ts) {
                if (!start) { start = ts; }
                var p = Math.min((ts - start) / dur, 1);
                var eased = 1 - Math.pow(1 - p, 3);
                el.textContent = Math.round(target * eased).toLocaleString();
                if (p < 1) { window.requestAnimationFrame(step); }
            }
            window.requestAnimationFrame(step);
        }

        if (!supportsIO) { nums.forEach(animate); return; }
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) { animate(entry.target); io.unobserve(entry.target); }
            });
        }, { threshold: 0.4 });
        nums.forEach(function (n) { io.observe(n); });
    }

    /* ----------------------------------------------------- password UX */
    /* One handler owns every password toggle and strength meter. The required
       checks mirror the server validator exactly: 8+ characters, a letter and a
       number. Upper/lowercase variety and a symbol are optional strength boosts. */

    var COMMON_PASSWORDS = [
        "password", "passw0rd", "12345678", "123456789", "qwerty", "qwertyui",
        "letmein", "welcome", "admin123", "iloveyou", "abc12345", "football",
        "monkey", "dragon", "sunshine", "princess", "michael", "shadow"
    ];

    function passwordRules(value) {
        var v = value || "";
        var upper = /[A-Z]/.test(v);
        var lower = /[a-z]/.test(v);
        var number = /[0-9]/.test(v);
        return {
            length: v.length >= 8,
            letter: upper || lower,
            upper: upper,
            lower: lower,
            mixed: upper && lower,
            number: number,
            symbol: /[^A-Za-z0-9]/.test(v)
        };
    }

    /* 0 = very weak ... 4 = strong. */
    /* True when the letters/digits are almost entirely one ascending run, or a
       keyboard walk ("qwerty", "asdfgh"). Symbols and case are ignored here. */
    function isStraightRun(value) {
        var s = (value || "").replace(/[^a-z0-9]/g, "");
        if (s.length < 4) { return false; }
        if (/^(qwerty|asdfgh|zxcvbn|qazwsx)/.test(s)) { return true; }

        var run = 1;
        var best = 1;
        for (var i = 1; i < s.length; i++) {
            run = s.charCodeAt(i) === s.charCodeAt(i - 1) + 1 ? run + 1 : 1;
            if (run > best) { best = run; }
        }
        return best >= Math.max(4, s.length - 1);
    }

    function passwordStrengthScore(value) {
        var v = value || "";
        if (!v) { return 0; }

        var r = passwordRules(v);
        var score = 0;

        if (r.length) { score++; }
        // This is the same minimum accepted by the server-side RegularExpression.
        if (r.letter && r.number) { score++; }
        if (v.length >= 12) { score++; }
        if (r.mixed) { score++; }
        if (r.symbol) { score++; }

        // A single repeated character or a well-known password never deserves more than
        // "Weak", however long it is. A straight run ("abcdefgh", "12345678") is capped at
        // "Fair" rather than "Weak", so a long password that merely starts with "abcd"
        // is not punished as harshly as "password" itself.
        var lower = v.toLowerCase();
        var repeated = /^(.)\1+$/.test(v);
        var common = COMMON_PASSWORDS.indexOf(lower) !== -1;
        if (repeated || common) { score = Math.min(score, 1); }
        else if (isStraightRun(lower)) { score = Math.min(score, 2); }

        return Math.max(0, Math.min(score, 4));
    }

    function initPasswords() {
        var labels = ["Very weak", "Weak", "Fair", "Good", "Strong"];

        /* ---- show / hide ------------------------------------------------- */
        function setEye(btn, input, visible) {
            input.type = visible ? "text" : "password";
            btn.classList.toggle("is-visible", visible);
            btn.setAttribute("aria-label", visible ? "Hide password" : "Show password");
            btn.setAttribute("aria-pressed", visible ? "true" : "false");
            btn.setAttribute("title", visible ? "Hide password" : "Show password");

            var on = btn.querySelector(".icon-eye");
            var off = btn.querySelector(".icon-eye-off");
            if (on) { on.style.display = visible ? "none" : "block"; }
            if (off) { off.style.display = visible ? "block" : "none"; }
        }

        document.querySelectorAll(".password-toggle").forEach(function (btn) {
            var field = btn.closest(".password-field");
            var input = field ? field.querySelector("input") : null;
            if (!input) { return; }

            setEye(btn, input, input.type === "text");

            btn.addEventListener("click", function (e) {
                e.preventDefault();
                setEye(btn, input, input.type === "password");
                // Keep the caret where it was, otherwise the field jumps to the end.
                var end = input.value.length;
                input.focus({ preventScroll: true });
                try { input.setSelectionRange(end, end); } catch (err) { /* type=email etc. */ }
            });
        });

        /* ---- strength meters --------------------------------------------- */
        document.querySelectorAll("[data-strength-for]").forEach(function (meter) {
            var input = document.getElementById(meter.getAttribute("data-strength-for"));
            var fill = meter.querySelector("[data-strength-fill]");
            var label = meter.querySelector("[data-strength-label]");
            if (!input || !fill || !label) { return; }

            var rules = meter.parentNode.querySelector("[data-pwd-rules]");
            var ruleItems = rules ? rules.querySelectorAll("li[data-rule]") : [];

            if (!label.hasAttribute("aria-live")) {
                label.setAttribute("aria-live", "polite");
            }

            function update() {
                var value = input.value;
                var r = passwordRules(value);

                if (!value) {
                    fill.className = "pwd-strength-fill";
                    fill.style.width = "0%";
                    label.textContent = "";
                    ruleItems.forEach(function (li) { li.classList.remove("is-met"); });
                    return;
                }

                var score = passwordStrengthScore(value);
                fill.className = "pwd-strength-fill pwd-strength-" + score;
                fill.style.width = ((score + 1) / 5 * 100) + "%";
                label.textContent = labels[score];
                label.setAttribute("data-score", String(score));

                ruleItems.forEach(function (li) {
                    var rule = li.getAttribute("data-rule");
                    li.classList.toggle("is-met", !!r[rule]);
                });
            }

            input.addEventListener("input", update);
            input.addEventListener("focus", update);
            update();
        });

        /* ---- confirm-match notes ----------------------------------------- */
        document.querySelectorAll("[data-pwd-match]").forEach(function (note) {
            var confirm = document.getElementById(note.getAttribute("data-pwd-match"));
            var target = document.getElementById(note.getAttribute("data-pwd-target"));
            if (!confirm || !target) { return; }

            function update() {
                if (!confirm.value) {
                    note.textContent = "";
                    note.className = "pwd-match-note";
                    return;
                }
                var ok = confirm.value === target.value;
                note.textContent = ok ? "Passwords match" : "Passwords do not match yet";
                note.className = "pwd-match-note " + (ok ? "is-ok" : "is-bad");
            }

            confirm.addEventListener("input", update);
            target.addEventListener("input", update);
            update();
        });

        /* ---- caps-lock hint ---------------------------------------------- */
        document.querySelectorAll(".password-field input").forEach(function (input) {
            var hint = input.parentNode.querySelector("[data-caps-hint]");
            if (!hint) { return; }
            input.addEventListener("keyup", function (e) {
                if (typeof e.getModifierState !== "function") { return; }
                hint.hidden = !e.getModifierState("CapsLock");
            });
            input.addEventListener("blur", function () { hint.hidden = true; });
        });

        /* ---- submit guard: only for forms that opt in -------------------- */
        document.querySelectorAll("form[data-pwd-guard]").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                var meter = form.querySelector("[data-strength-for]");
                var input = meter ? document.getElementById(meter.getAttribute("data-strength-for")) : null;
                if (!input || !input.value) { return; }

                var rules = passwordRules(input.value);
                if (!rules.length || !rules.letter || !rules.number) {
                    e.preventDefault();
                    input.focus();
                    var label = meter.querySelector("[data-strength-label]");
                    if (label) { label.textContent = "Use 8+ characters, including a letter and a number"; }
                }
            });
        });
    }


    /* ------------------------------------------------------------ form polish */
    function initForms() {
        /* Auto-dismiss success notices after 6s (ported from Quiz 1). */
        document.querySelectorAll(".alert-success").forEach(function (box) {
            window.setTimeout(function () {
                box.style.transition = "opacity .35s, transform .35s";
                box.style.opacity = "0";
                box.style.transform = "translateY(-6px)";
                window.setTimeout(function () { box.style.display = "none"; }, 380);
            }, 6000);
        });

        /* Prevent accidental double submits. Skip when another handler (e.g. the
           password guard) already cancelled the submit, so the button stays usable. */
        document.querySelectorAll("form").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                if (e.defaultPrevented) { return; }
                var btn = form.querySelector("button[type=submit]");
                if (!btn || btn.disabled) { return; }
                window.setTimeout(function () { btn.disabled = true; }, 0);
            });
        });

        /* Numeric-only phone fields. */
        document.querySelectorAll("input[type=tel][inputmode=numeric]").forEach(function (input) {
            input.addEventListener("input", function () {
                var clean = input.value.replace(/[^0-9]/g, "").slice(0, 11);
                if (clean !== input.value) { input.value = clean; }
            });
        });
    }

    /* --------------------------------------------------- products flavour filter */
    function initProductFilter() {
        var bar = document.querySelector("[data-mg-filter-bar]");
        if (!bar) { return; }
        var cards = document.querySelectorAll("[data-mg-product]");
        bar.addEventListener("click", function (e) {
            var btn = e.target.closest("[data-mg-filter]");
            if (!btn) { return; }
            var value = btn.getAttribute("data-mg-filter");
            bar.querySelectorAll("[data-mg-filter]").forEach(function (b) {
                b.classList.toggle("is-active", b === btn);
                b.setAttribute("aria-pressed", b === btn ? "true" : "false");
            });
            cards.forEach(function (card) {
                var show = !value || card.getAttribute("data-mg-flavor") === value;
                card.style.display = show ? "" : "none";
            });
        });
    }

    /* --------------------------------------------------- flavor carousel */
    function initFlavorCarousel() {
        document.querySelectorAll("[data-mg-flavor-carousel]").forEach(function (carousel) {
            var slides = Array.prototype.slice.call(carousel.querySelectorAll("[data-flavor-slide]"));
            var dots = Array.prototype.slice.call(carousel.querySelectorAll("[data-flavor-dot]"));
            var previous = carousel.querySelector("[data-flavor-prev]");
            var next = carousel.querySelector("[data-flavor-next]");
            var currentLabel = carousel.querySelector("[data-flavor-current]");
            var band = carousel.querySelector(".mm-flavor-carousel");
            if (slides.length < 2) {
                if (previous) { previous.hidden = true; }
                if (next) { next.hidden = true; }
                dots.forEach(function (dot) { dot.hidden = true; });
                return;
            }

            var active = 0;
            function show(index) {
                active = (index + slides.length) % slides.length;
                slides.forEach(function (slide, i) {
                    var selected = i === active;
                    slide.hidden = !selected;
                    slide.classList.toggle("is-active", selected);
                    slide.setAttribute("aria-hidden", selected ? "false" : "true");
                });
                dots.forEach(function (dot, i) {
                    var selected = i === active;
                    dot.classList.toggle("is-active", selected);
                    dot.setAttribute("aria-pressed", selected ? "true" : "false");
                });
                if (currentLabel) { currentLabel.textContent = String(active + 1).padStart(2, "0"); }

                // The strips around the slide (and the arrows) follow the active drink's colour.
                if (band) {
                    var bg = slides[active].getAttribute("data-bg");
                    if (bg) { band.style.setProperty("--carousel-bg", bg); }
                    else { band.style.removeProperty("--carousel-bg"); }
                    band.classList.toggle("is-dark", slides[active].classList.contains("is-dark"));
                }
            }

            if (previous) { previous.addEventListener("click", function () { show(active - 1); }); }
            if (next) { next.addEventListener("click", function () { show(active + 1); }); }
            dots.forEach(function (dot) {
                dot.addEventListener("click", function () {
                    var index = parseInt(dot.getAttribute("data-flavor-dot"), 10);
                    if (!isNaN(index)) { show(index); }
                });
            });

            carousel.addEventListener("keydown", function (e) {
                if (e.key === "ArrowLeft") { show(active - 1); }
                if (e.key === "ArrowRight") { show(active + 1); }
            });
            show(0);
        });
    }

    /* ------------------------------------------------------- smooth anchor nav */
    function initAnchors() {
        document.addEventListener("click", function (e) {
            var a = e.target.closest('a[href^="#"]');
            if (!a) { return; }
            var id = a.getAttribute("href");
            if (!id || id === "#") { return; }
            var target = document.querySelector(id);
            if (!target) { return; }
            e.preventDefault();
            var top = target.getBoundingClientRect().top + window.pageYOffset - 90;
            window.scrollTo({ top: top, behavior: reduceMotion ? "auto" : "smooth" });
            history.replaceState(null, "", id);
        });
    }

    ready(function () {
        initChrome();
        initMarquees();
        initReveal();
        initMagnetic();
        initTilt();
        initCounters();
        initPasswords();
        initForms();
        initProductFilter();
        initFlavorCarousel();
        initAnchors();
    });
})();
