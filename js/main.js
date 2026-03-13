// Mobile nav toggle
const navToggle = document.getElementById("navToggle");
const navMenu = document.getElementById("navMenu");

if (navToggle && navMenu) {
  navToggle.addEventListener("click", () => {
    const open = navMenu.classList.toggle("is-open");
    navToggle.setAttribute("aria-expanded", String(open));
  });

  // Close menu on link click (mobile)
  navMenu.querySelectorAll("a").forEach(a => {
    a.addEventListener("click", () => {
      navMenu.classList.remove("is-open");
      navToggle.setAttribute("aria-expanded", "false");
    });
  });
}

// Booking form success state (prototype)
const form = document.getElementById("bookingForm");
const success = document.getElementById("formSuccess");
if (form && success) {
  form.addEventListener("submit", (e) => {
    e.preventDefault();
    success.hidden = false;
    form.reset();
    success.scrollIntoView({ behavior: "smooth", block: "center" });
  });
}

// Lightbox for preview grid
const lightbox = document.getElementById("lightbox");
const lightboxImg = document.getElementById("lightboxImg");
const lightboxClose = document.getElementById("lightboxClose");

function openLightbox(src) {
  if (!lightbox || !lightboxImg) return;
  lightboxImg.src = src;
  lightbox.hidden = false;
  document.body.style.overflow = "hidden";
}

function closeLightbox() {
  if (!lightbox) return;
  lightbox.hidden = true;
  document.body.style.overflow = "";
  if (lightboxImg) lightboxImg.src = "";
}

document.querySelectorAll(".thumb[data-full]").forEach(btn => {
  btn.addEventListener("click", () => openLightbox(btn.dataset.full));
});

if (lightboxClose) lightboxClose.addEventListener("click", closeLightbox);
if (lightbox) {
  lightbox.addEventListener("click", (e) => {
    if (e.target === lightbox) closeLightbox();
  });
  window.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && !lightbox.hidden) closeLightbox();
  });
}

// Reveal packages section only when Packages button is clicked
const packagesTrigger = document.getElementById("packagesTrigger");
const packagesSection = document.getElementById("packages");

if (packagesTrigger && packagesSection) {
  packagesTrigger.addEventListener("click", (e) => {
    e.preventDefault();
    if (packagesSection.hasAttribute("hidden")) {
      packagesSection.removeAttribute("hidden");
      packagesSection.scrollIntoView({ behavior: "smooth", block: "start" });
    } else {
      packagesSection.setAttribute("hidden", "");
    }
  });
}

// Background music: start after 2 seconds with a soft fade-in.
const bgMusic = document.getElementById("bgMusic");
const entryGate = document.getElementById("entryGate");
const enterSiteBtn = document.getElementById("enterSiteBtn");

function startMusicWithFade() {
  if (!bgMusic) return;
  bgMusic.volume = 0;
  bgMusic.play().then(() => {
    const targetVolume = 0.4;
    const step = 0.04;
    const timer = setInterval(() => {
      const nextVolume = Math.min(targetVolume, bgMusic.volume + step);
      bgMusic.volume = nextVolume;
      if (nextVolume >= targetVolume) clearInterval(timer);
    }, 180);
  }).catch(() => {
    // Autoplay can be blocked; retry on first user interaction.
    const unlock = () => {
      startMusicWithFade();
      window.removeEventListener("click", unlock);
      window.removeEventListener("keydown", unlock);
      window.removeEventListener("touchstart", unlock);
    };
    window.addEventListener("click", unlock, { once: true });
    window.addEventListener("keydown", unlock, { once: true });
    window.addEventListener("touchstart", unlock, { once: true });
  });
}

if (entryGate) {
  document.body.style.overflow = "hidden";
}

if (enterSiteBtn) {
  enterSiteBtn.addEventListener("click", () => {
    if (entryGate) {
      entryGate.hidden = true;
      document.body.style.overflow = "";
    }
    setTimeout(startMusicWithFade, 200);
  });
} else if (bgMusic) {
  setTimeout(startMusicWithFade, 2000);
}
