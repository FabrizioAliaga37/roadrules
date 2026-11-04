<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const props = defineProps({ apiBase: { type: String, required: true } })

const jurisdictions = ref([])
const code = ref('')
const axles = ref(1)
const result = ref(null)
const error = ref(null)

// Display-only state; it doesn't change what the calculator does.
const busy = ref(false)
const resultAxles = ref(1)

const groups = [
  { count: 1, name: 'Single' },
  { count: 2, name: 'Tandem' },
  { count: 3, name: 'Tridem' }
]

// Behaviour captured for the Blazor conversion (see README): the button is disabled
// by this computed property, not by validation messages.
const canSubmit = computed(() => code.value !== '' && axles.value >= 1 && axles.value <= 3)

const jurisdictionName = computed(() =>
  jurisdictions.value.find(j => j.code === result.value?.jurisdictionCode)?.name
  ?? result.value?.jurisdictionCode)

const pounds = computed(() => Math.round((result.value?.maxKg ?? 0) * 2.20462))

// Truck wheels for the result: highlighted axles centred under the trailer.
const wheelXs = computed(() => {
  const n = resultAxles.value
  const start = 66 - (n - 1) * 14
  return Array.from({ length: n }, (_, i) => start + i * 28)
})

// Known flaw kept on purpose: a failed load leaves an empty dropdown with no message.
// The Blazor version shows an error instead (NOTES.md, step 13d).
onMounted(async () => {
  const response = await axios.get(`${props.apiBase}/api/jurisdictions`)
  jurisdictions.value = response.data
})

async function calculate() {
  result.value = null
  error.value = null
  busy.value = true
  try {
    const response = await axios.get(`${props.apiBase}/api/calculate`, {
      params: { code: code.value, axles: axles.value }
    })
    resultAxles.value = axles.value
    result.value = response.data
  } catch (e) {
    error.value = e.response?.data?.message ?? 'Request failed'
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="rr-page-header">
    <div class="rr-eyebrow">
      Tools
      <span class="rr-pill rr-pill-vue"><span class="rr-dot"></span>Vue 3</span>
    </div>
    <h1 class="rr-title">Weight calculator (Vue)</h1>
    <p class="rr-subtitle">
      The original Vue version of the calculator. It calls <code>/api/calculate</code> on the
      RoadRules app for the limit in effect today.
    </p>
  </div>

  <div class="rr-calc">
    <div class="rr-card rr-card-body rr-fade">
      <h2 class="rr-card-title">Vehicle details</h2>

      <form @submit.prevent="calculate">
        <div class="rr-field">
          <label class="form-label" for="vue-jurisdiction">Jurisdiction</label>
          <select id="vue-jurisdiction" class="form-control" v-model="code">
            <option value="">Select...</option>
            <option v-for="j in jurisdictions" :key="j.code" :value="j.code">{{ j.name }}</option>
          </select>
        </div>

        <div class="rr-field">
          <label class="form-label" for="vue-axles">Axles in the group</label>
          <input id="vue-axles" class="form-control" type="number" v-model.number="axles" min="1" max="3" />

          <div class="rr-segment" role="group" aria-label="Common axle groups">
            <button
              v-for="g in groups"
              :key="g.count"
              type="button"
              :class="{ active: axles === g.count }"
              :aria-pressed="axles === g.count"
              @click="axles = g.count"
            >
              <span class="rr-axle-dots"><i v-for="n in g.count" :key="n"></i></span>
              {{ g.name }}
              <small>{{ g.count }} axle{{ g.count === 1 ? '' : 's' }}</small>
            </button>
          </div>
        </div>

        <button class="btn btn-primary btn-lg" :disabled="!canSubmit">
          <span v-if="busy" class="rr-spinner" role="status" aria-label="Calculating"></span>
          Calculate
        </button>
      </form>
    </div>

    <div class="rr-card rr-card-body rr-result">
      <div v-if="result" :key="`${result.jurisdictionCode}-${resultAxles}`" class="rr-fade">
        <div class="rr-result-hero">
          <div class="rr-result-label">
            Maximum for a {{ result.axleGroup.toLowerCase() }} group in {{ result.jurisdictionCode }}
          </div>
          <div class="rr-result-value">
            {{ result.maxKg.toLocaleString() }}<span class="rr-result-unit">kg</span>
          </div>
          <div class="rr-result-sub">
            ≈ {{ pounds.toLocaleString() }} lb
            <template v-if="result.effectiveFrom"> · in effect since {{ result.effectiveFrom }}</template>
          </div>

          <svg class="rr-truck" viewBox="0 0 300 96" role="img"
               :aria-label="`${resultAxles} axle ${result.axleGroup.toLowerCase()} group highlighted`">
            <line x1="0" y1="88" x2="300" y2="88" stroke="#c7d2fe" stroke-width="2" stroke-dasharray="10 8" />
            <rect x="8" y="12" width="206" height="48" rx="7" fill="#fff" stroke="#c7d2fe" stroke-width="2" />
            <rect x="222" y="30" width="58" height="34" rx="8" fill="#fff" stroke="#c7d2fe" stroke-width="2" />
            <path d="M256 30v-8a6 6 0 0 1 6-6h6l12 14" fill="none" stroke="#c7d2fe" stroke-width="2" stroke-linejoin="round" />
            <rect x="208" y="54" width="20" height="6" rx="2" fill="#c7d2fe" />
            <circle cx="262" cy="74" r="12" fill="#e0e7ff" /><circle cx="262" cy="74" r="4" fill="#fff" />
            <template v-for="x in wheelXs" :key="x">
              <circle :cx="x" cy="74" r="12" fill="#4f46e5" />
              <circle :cx="x" cy="74" r="4" fill="#fff" />
            </template>
          </svg>
        </div>

        <div class="rr-meta">
          <div><span>Jurisdiction</span><strong>{{ jurisdictionName }}</strong></div>
          <div><span>Axle group</span><strong>{{ result.axleGroup }} ({{ resultAxles }})</strong></div>
          <div><span>In effect since</span><strong>{{ result.effectiveFrom ?? '—' }}</strong></div>
        </div>
      </div>

      <div v-else-if="error" class="rr-empty rr-fade">
        <div class="rr-alert-error" role="alert">
          <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" /><path d="M12 8v4.5M12 16h.01" /></svg>
          <div>
            <strong>No limit found</strong>
            {{ error }}
          </div>
        </div>
      </div>

      <div v-else class="rr-empty">
        <div class="rr-empty-icon">
          <svg viewBox="0 0 24 24"><path d="M2.5 6.5h11v9h-11z" /><path d="M13.5 9.5h4l3.5 3.5v2.5h-7.5" /><circle cx="6.5" cy="17.5" r="2" /><circle cx="17" cy="17.5" r="2" /></svg>
        </div>
        <strong>No result yet</strong>
        <span>Choose a jurisdiction and an axle group, then press Calculate.</span>
      </div>
    </div>
  </div>
</template>
