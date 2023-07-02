#version 330 core
out vec4 FragColor;

in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;
uniform vec3 lightPos;

//手电筒
struct SpotLight
{
	vec3 direction;
	vec3 position;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;

	float cutOff;
	float outerCutOff;
};

uniform SpotLight spotLight;
uniform sampler2D texture_diffuse1;
uniform vec3 viewPos;

vec3 CalcSpotLight(SpotLight light,vec3 normal,vec3 FragPos,vec3 viewDir);
void main()
{   
    //给他加一点点环境光
    vec3 ambient =0.1 * texture(texture_diffuse1,TexCoords).rgb;

    vec3 lightColor=vec3(1.0,1.0,1.0);
    vec3 normal = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(normal,lightDir),0.0);

    //加个手电筒
	vec3 viewDir = normalize(viewPos - FragPos);

    vec3 result =ambient + lightColor * diff * texture(texture_diffuse1, TexCoords).rgb;
	result += CalcSpotLight(spotLight, normal, FragPos, viewDir);    
    FragColor = vec4(result,1.0);
}

vec3 CalcSpotLight(SpotLight light,vec3 normal,vec3 FragPos,vec3 viewDir)
{
	vec3 lightDir = normalize(light.position - FragPos);
	float theta = dot(normalize(-light.direction),lightDir);
	float epsilon = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff)/epsilon,0.0,1.0);

	//漫反射
	float diff = max(dot(normal,lightDir),0.0);

	//镜面光
	vec3 reflectDir = reflect(-lightDir,normal);
	float spec = pow(max(dot(reflectDir,viewDir),0.0),64.0);

	//衰弱
	float distance = length(light.position - FragPos);
	float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * distance * distance);

	//合并
	vec3 ambient = light.ambient * texture(texture_diffuse1,TexCoords).rgb;
	vec3 diffuse = spotLight.diffuse * diff * texture(texture_diffuse1,TexCoords).rgb;
	ambient *=attenuation * intensity;
	diffuse *=attenuation * intensity;

	return ambient + diffuse;
}