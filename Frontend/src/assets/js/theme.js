function applyTheme() {
  const theme = localStorage.getItem("theme") || "light";
  const btn = document.getElementById("themeToggle");

  if (theme === "dark") {
    document.body.classList.add("dark-mode");
    if (btn) btn.innerHTML = '<i class="fa-solid fa-sun"></i>';
  } else {
    document.body.classList.remove("dark-mode");
    if (btn) btn.innerHTML = '<i class="fa-solid fa-moon"></i>';
  }
}

function toggleTheme() {
  const isDark = document.body.classList.contains("dark-mode");
  localStorage.setItem("theme", isDark ? "light" : "dark");
  applyTheme();
}

document.addEventListener("DOMContentLoaded", applyTheme);