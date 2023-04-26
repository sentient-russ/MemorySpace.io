document.addEventListener('DOMContentLoaded', () => {
    const cleave = new Cleave('#Phone', {
        numericOnly: true,
        blocks: [0, 3, 0, 3, 4],
        delimiters: ["(", ")", " ", "-"],
        phoneRegionCode: 'US'
    });
});