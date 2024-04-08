#version 300 es

in vec2 aPos;

uniform mat4 uProjMatrix;
uniform mat4 uModelViewMatrix;

void main() {
    gl_Position = uProjMatrix * uModelViewMatrix * vec4(aPos, 0.0, 1.0);
}