// Canvas rendering for Hunger Game simulation
let canvas = null;
let ctx = null;
let canvasScale = 1;
let showVisionCones = false;
let showVelocityVectors = false;
let showIds = false;

// Initialize the canvas
window.canvasInterop = {
    initCanvas: function(canvasId, width, height) {
        canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas not found:', canvasId);
            return false;
        }
        
        ctx = canvas.getContext('2d');
        canvas.width = width;
        canvas.height = height;
        
        // Make canvas responsive
        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());
        
        return true;
    },

    resizeCanvas: function() {
        if (!canvas) return;
        
        const container = canvas.parentElement;
        const containerWidth = container.clientWidth - 32; // Account for padding
        const containerHeight = container.clientHeight - 32;
        
        // Maintain aspect ratio
        const aspectRatio = canvas.width / canvas.height;
        let displayWidth = containerWidth;
        let displayHeight = containerWidth / aspectRatio;
        
        if (displayHeight > containerHeight) {
            displayHeight = containerHeight;
            displayWidth = containerHeight * aspectRatio;
        }
        
        canvas.style.width = displayWidth + 'px';
        canvas.style.height = displayHeight + 'px';
        
        canvasScale = displayWidth / canvas.width;
    },

    setRenderOptions: function(options) {
        showVisionCones = options.showVisionCones || false;
        showVelocityVectors = options.showVelocityVectors || false;
        showIds = options.showIds || false;
    },

    drawFrame: function(frameData) {
        if (!ctx || !canvas) return;
        
        // Clear canvas
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        
        // Set up coordinate system (arena coordinates to canvas coordinates)
        const scaleX = canvas.width / frameData.arenaWidth;
        const scaleY = canvas.height / frameData.arenaHeight;
        
        // Draw background
        ctx.fillStyle = '#2a4a3a'; // Dark green
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        // Draw obstacles
        this.drawObstacles(frameData.obstacles, scaleX, scaleY);
        
        // Draw animals
        this.drawAnimals(frameData.hares, 'hare', scaleX, scaleY);
        this.drawAnimals(frameData.lynxes, 'lynx', scaleX, scaleY);
    },

    drawObstacles: function(obstacles, scaleX, scaleY) {
        obstacles.forEach(obstacle => {
            const x = obstacle.x * scaleX;
            const y = obstacle.y * scaleY;
            const width = obstacle.width * scaleX;
            const height = obstacle.height * scaleY;
            
            ctx.save();
            
            switch (obstacle.type) {
                case 'Tree':
                    ctx.fillStyle = '#8B4513'; // Brown
                    break;
                case 'Shrub':
                    ctx.fillStyle = '#228B22'; // Forest green
                    break;
                case 'Water':
                    ctx.fillStyle = '#4169E1'; // Royal blue
                    break;
                case 'Grass':
                    ctx.fillStyle = '#32CD32'; // Lime green
                    break;
                case 'Dirt':
                    ctx.fillStyle = '#8B7355'; // Burlywood
                    break;
                default:
                    ctx.fillStyle = '#666666'; // Gray
            }
            
            // Draw obstacle
            if (obstacle.type === 'Tree') {
                // Draw tree as circle
                ctx.beginPath();
                ctx.arc(x, y, Math.max(width, height) / 2, 0, 2 * Math.PI);
                ctx.fill();
            } else if (obstacle.type === 'Water') {
                // Draw water with wavy edges
                ctx.beginPath();
                ctx.ellipse(x, y, width / 2, height / 2, 0, 0, 2 * Math.PI);
                ctx.fill();
            } else {
                // Draw as rectangle
                ctx.fillRect(x - width / 2, y - height / 2, width, height);
            }
            
            ctx.restore();
        });
    },

    drawAnimals: function(animals, type, scaleX, scaleY) {
        animals.forEach(animal => {
            const x = animal.x * scaleX;
            const y = animal.y * scaleY;
            const vx = animal.vx;
            const vy = animal.vy;
            
            ctx.save();
            
            // Choose color based on type and energy
            if (type === 'hare') {
                const energyRatio = animal.energy / 75; // Assuming max energy ~75
                const green = Math.floor(100 + energyRatio * 155);
                ctx.fillStyle = `rgb(139, ${green}, 69)`; // Brown to green based on energy
            } else {
                const energyRatio = animal.energy / 100; // Assuming max energy ~100
                const red = Math.floor(100 + energyRatio * 155);
                ctx.fillStyle = `rgb(${red}, 69, 0)`; // Dark red to bright red based on energy
            }
            
            // Draw animal body
            const size = type === 'hare' ? 6 : 8;
            ctx.beginPath();
            ctx.arc(x, y, size, 0, 2 * Math.PI);
            ctx.fill();
            
            // Draw direction indicator
            if (vx !== 0 || vy !== 0) {
                const angle = Math.atan2(vy, vx);
                const length = size * 1.5;
                
                ctx.strokeStyle = ctx.fillStyle;
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.moveTo(x, y);
                ctx.lineTo(x + Math.cos(angle) * length, y + Math.sin(angle) * length);
                ctx.stroke();
            }
            
            // Draw velocity vector if enabled
            if (showVelocityVectors && (vx !== 0 || vy !== 0)) {
                const velocityScale = 10; // Scale factor for visibility
                ctx.strokeStyle = '#FFFF00'; // Yellow
                ctx.lineWidth = 1;
                ctx.setLineDash([2, 2]);
                ctx.beginPath();
                ctx.moveTo(x, y);
                ctx.lineTo(x + vx * velocityScale, y + vy * velocityScale);
                ctx.stroke();
                ctx.setLineDash([]);
            }
            
            // Draw vision cone if enabled
            if (showVisionCones) {
                const visionRange = type === 'hare' ? 40 : 35;
                const visionAngle = Math.PI / 3; // 60 degrees
                const facing = Math.atan2(vy, vx);
                
                ctx.fillStyle = type === 'hare' ? 'rgba(0, 255, 0, 0.1)' : 'rgba(255, 0, 0, 0.1)';
                ctx.beginPath();
                ctx.moveTo(x, y);
                ctx.arc(x, y, visionRange * scaleX, facing - visionAngle / 2, facing + visionAngle / 2);
                ctx.closePath();
                ctx.fill();
            }
            
            // Draw ID if enabled
            if (showIds && animal.name) {
                ctx.fillStyle = '#FFFFFF';
                ctx.font = '10px Arial';
                ctx.textAlign = 'center';
                ctx.fillText(animal.name, x, y - size - 5);
            }
            
            ctx.restore();
        });
    }
};

// Initialize canvas when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('Canvas interop loaded');
}); 