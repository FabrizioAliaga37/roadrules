<script setup>
import Calculator from './components/Calculator.vue'

// The http:// URL that `dotnet run --project RoadRules` prints (launchSettings "http" profile).
// Change it here if your app runs on another port.
const apiBase = 'https://localhost:7292'
</script>

<template>
  <div class="rr-shell">
    <aside class="rr-sidebar">
      <a class="rr-brand" href="/">
        <span class="rr-logo" aria-hidden="true">
          <svg viewBox="0 0 24 24"><path d="M2.5 6.5h11v9h-11z" /><path d="M13.5 9.5h4l3.5 3.5v2.5h-7.5" /><circle cx="6.5" cy="17.5" r="2" /><circle cx="17" cy="17.5" r="2" /></svg>
        </span>
        <span>
          <strong>RoadRules</strong>
          <small>Vue version</small>
        </span>
      </a>

      <nav class="rr-nav">
        <div class="rr-nav-label">Vue</div>
        <a class="rr-link active" href="/">
          <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="5" y="2.5" width="14" height="19" rx="2.5" /><path d="M8.5 6.5h7M8.5 11h.01M12 11h.01M15.5 11h.01M8.5 14.5h.01M12 14.5h.01M15.5 14.5h.01M8.5 18h.01M12 18h.01M15.5 18h.01" /></svg>
          <span>Calculator</span>
        </a>
      </nav>

      <div class="rr-nav-foot">
        Illustrative numbers only.<br />Not real axle limits.
      </div>
    </aside>

    <div class="rr-main">
      <header class="rr-topbar">
        <nav class="rr-crumbs" aria-label="Breadcrumb">
          <span>RoadRules</span>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m9 6 6 6-6 6" /></svg>
          <strong>Weight calculator (Vue)</strong>
        </nav>
        <span class="rr-pill rr-pill-warn"><span class="rr-dot"></span>Illustrative data, not real limits</span>
      </header>

      <main class="rr-content">
        <Calculator :api-base="apiBase" />
      </main>
    </div>
  </div>
</template>
