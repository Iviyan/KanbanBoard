<template>
<div class="modal fade" ref="modalEl" id="exampleModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" :class="{ ['modal-' + size]: size != null }">
    <div class="modal-content">
      <div class="modal-header">
		    <slot name="header" />
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <slot name="body" />
      </div>
      <div class="modal-footer">
		    <slot name="footer" />
      </div>
    </div>
  </div>
</div>
</template>

<script setup>
    import { onMounted, ref } from 'vue';
    import { Modal } from 'bootstrap';

    const props = defineProps({
        size: { type: String, required: false }
    })

    const modalEl = ref(null);
    var modal = null;

    onMounted(() => {
        modal = new Modal(modalEl.value, {});
    })

    const show = () => modal.show(); 
    const hide = () => modal.hide(); 

    defineExpose({
      modal,
      show,
      hide
    })
</script>

<style>

.modal-header {
  word-break: break-all;
}

</style>