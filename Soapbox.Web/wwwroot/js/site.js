// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
  // dark mode
}

window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
  const newColorScheme = e.matches ? "dark" : "light";
});

document.addEventListener('click', event => {
  const formId = event.target.getAttribute('submit-form');
  if (!formId)
    return;

  const form = Array.from(document.forms).find(form => form.id === formId);

  if (!form)
    return;

  form.submit();

  event.preventDefault();
  event.stopPropagation();
});
