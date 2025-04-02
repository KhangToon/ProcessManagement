window.printQRPalletComponent = function (componentId) {
    const componentToPrint = document.getElementById(componentId);
    if (!componentToPrint) {
        console.error(`Component with id "${componentId}" not found.`);
        return;
    }

    const printWindow = window.open('', '_blank');
    printWindow.document.write('<html><head><title>Print</title>');

    // Include all stylesheets
    document.querySelectorAll('link[rel="stylesheet"]').forEach(styleSheet => {
        printWindow.document.write(styleSheet.outerHTML);
    });

    // Optionally include internal styles if needed
    const styles = document.createElement('style');
    styles.innerHTML = `
        @media print {
            @page {
                margin: 0mm; /* Adjust margin as needed */
            }
            body {
                display: flex;
                justify-content: center;
                align-items: center;
                margin: 10px;
            }
            #${componentId} {
                text-align: center; /* Center the content */
            }
            .table {
                border-collapse: collapse;
                width: 100%;
            }
            .table td, .table th {
                border: 1px solid black !important;
                padding: 5px;
                text-align: left;
            }
        }
    `;
    printWindow.document.head.appendChild(styles);

    printWindow.document.write('</head><body>');

    printWindow.document.write(componentToPrint.outerHTML); // Write the component
    printWindow.document.write('</body></html>');

    printWindow.document.close();
    printWindow.onload = function () {
        printWindow.focus();
        printWindow.print();
        printWindow.close();
    };
}

