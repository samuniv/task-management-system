// File download utilities for Blazor WebAssembly

window.downloadFile = function (filename, content, contentType) {
    // Create a Blob with the content
    const blob = new Blob([content], { type: contentType || 'application/octet-stream' });
    
    // Create a URL for the blob
    const url = window.URL.createObjectURL(blob);
    
    // Create a temporary anchor element for download
    const anchor = document.createElement('a');
    anchor.style.display = 'none';
    anchor.href = url;
    anchor.download = filename;
    
    // Append to body, click, and remove
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    
    // Clean up the URL object
    window.URL.revokeObjectURL(url);
};

window.downloadFileFromUrl = function (filename, url) {
    const anchor = document.createElement('a');
    anchor.style.display = 'none';
    anchor.href = url;
    anchor.download = filename;
    
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
};
